#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace PhysicalMouseDemo
{
    // Only compiled in the Editor; added temporarily by the Run Checks menu, never saved in the scene.
    public sealed class DemoPlayChecks : MonoBehaviour
    {
        [Serializable] public class Result { public string check;public bool passed;public string detail; }
        [Serializable] public class Report { public List<Result> checks = new List<Result>();public int errors;public bool passed; }
        private readonly Report report = new Report();
        private readonly WaitForFixedUpdate tick = new WaitForFixedUpdate();
        private void OnEnable() { Application.logMessageReceived += Log; }
        private void OnDisable() { Application.logMessageReceived -= Log; }
        private void Log(string text,string stack,LogType type) { if (type==LogType.Error || type==LogType.Exception || type==LogType.Assert) report.errors++; }
        private void Check(string name,bool passed,string detail="") { report.checks.Add(new Result { check=name,passed=passed,detail=detail }); }
        private IEnumerator Start()
        {
            Application.runInBackground = true;
            var routine = Run();
            while (true)
            {
                object yielded;
                try { if (!routine.MoveNext()) break;yielded=routine.Current; }
                catch (Exception ex) { Check("Unexpected exception",false,ex.ToString());break; }
                yield return yielded;
            }
            report.passed = report.errors==0 && report.checks.TrueForAll(x=>x.passed);
            File.WriteAllText(Path.Combine(Application.dataPath,"../Logs/PhysicalMouseDemo-tests.json"),JsonUtility.ToJson(report,true));
            Debug.Log("PhysicalMouseDemo checks: " + (report.passed ? "PASS" : "FAIL") + " (" + report.checks.Count + " checks)");
            EditorApplication.isPlaying = false;
        }
        private IEnumerator Run()
        {
            var input=FindFirstObjectByType<PlayerInputController>();var hand=FindFirstObjectByType<PhysicalHandController>();var grab=hand.GetComponent<GrabInteractor>();var look=FindFirstObjectByType<CameraLookController>();
            input.enabled=false;look.enabled=false;
            Check("Input System bindings exist",!string.IsNullOrEmpty(input.LookBinding)&&!string.IsNullOrEmpty(input.GrabBinding)&&!string.IsNullOrEmpty(input.PointBinding)&&!string.IsNullOrEmpty(input.SelectionBinding));
            float depthBefore=hand.Depth;hand.MovePointer(new Vector2(0,40),true);
            Check("RMB depth drag moves away",hand.Depth>depthBefore);
            hand.MovePointer(new Vector2(0,-40),true);Check("RMB depth drag moves closer",Mathf.Abs(hand.Depth-depthBefore)<.001f);
            hand.AddPointerDelta(new Vector2(100000,100000));Check("Hand target bounds",hand.ReachOffset.x<=1.051f&&hand.ReachOffset.y<=.701f);
            hand.AimAtWorldPoint(new Vector3(0,1.1f,0.1f));for(int i=0;i<45;i++)yield return tick;
            var visual=hand.GetComponentInChildren<HandVisual>();
            Check("Animated hand model present",visual && visual.GetComponentsInChildren<Renderer>().Length>=39 && !hand.GetComponent<Renderer>().enabled);
            Vector3 before=hand.Body.position;hand.AddPointerDelta(new Vector2(60,0));for(int i=0;i<30;i++)yield return tick;
            Check("Mouse delta moves physical hand",hand.Body.position.x>before.x+.08f,hand.Body.position.ToString());
            Check("Hand remains dynamic and bounded",!hand.Body.isKinematic&&hand.transform.parent==null&&Vector3.Distance(hand.Body.position,hand.Target)<.2f);
            float lightLift=0,heavyLift=0;
            foreach(string name in new[]{"GrabCube","HeavyCube"})
            {
                var rb=GameObject.Find(name).GetComponent<Rigidbody>();
                hand.AimAtWorldPoint(rb.position+Vector3.up*.23f);for(int i=0;i<65;i++)yield return tick;
                bool got=grab.TryGrab();Check(name+" grabs",got&&grab.Held.Body==rb,"Hand "+hand.Body.position+" body "+rb.position);
                if(!got)continue;
                Check(name+" remains a dynamic Rigidbody",!rb.isKinematic&&rb.GetComponent<ConfigurableJoint>()!=null);
                float start=rb.position.y;hand.AimAtWorldPoint(hand.Target+Vector3.up*.32f);
                for(int i=0;i<15;i++)yield return tick;
                Check(name+" hand closes",visual && visual.GripBlend>.9f);
                float lift=rb.position.y-start;if(name=="GrabCube")lightLift=lift;else heavyLift=lift;
                Vector3 velocity=rb.linearVelocity;grab.Release();
                Check(name+" release preserves velocity",(rb.linearVelocity-velocity).sqrMagnitude<.000001f,"speed "+velocity.magnitude);
                for(int i=0;i<35;i++)yield return tick;
                Check(name+" release removes only the grab joint",!grab.IsHolding&&!rb.GetComponent<ConfigurableJoint>()&&!rb.isKinematic);
            }
            Check("Heavy cube responds more slowly",lightLift>.02f&&heavyLift<lightLift*.85f,"light lift "+lightLift+"; heavy lift "+heavyLift);
            foreach(string path in new[]{"Environment/LeverDemo/Handle","Environment/AircraftControlDemo/Handle"})
            {
                var control=GameObject.Find(path);var rb=control.GetComponent<Rigidbody>();var feedback=control.GetComponent<ConstrainedInteractable>();
                Vector3 handleTop=control.transform.TransformPoint(Vector3.up*.25f);
                hand.AimAtWorldPoint(handleTop+Vector3.up*.19f);for(int i=0;i<65;i++)yield return tick;
                bool got=grab.TryGrab();Check(path+" grabs through shared interactor",got&&grab.Held.Body==rb);
                if(!got)continue;
                var pivot=Camera.main.transform.parent;Quaternion savedView=pivot.rotation;pivot.rotation=Quaternion.Euler(0,65,0);
                Vector3 localBefore=hand.Target;hand.MovePointer(new Vector2(20,20),false);for(int i=0;i<20;i++)yield return tick;
                Vector3 expected=control.GetComponent<Grabbable>().MovementFrame.TransformVector(new Vector3(.05f,0,path.Contains("Lever")?0:.05f));
                Check(path+" uses object axes with camera turned",Vector3.Distance(hand.Target-localBefore,expected)<.01f);pivot.rotation=savedView;
                Vector3 offset=path.Contains("Lever")?new Vector3(-.3f,-.08f,0):new Vector3(.17f,-.08f,.17f);
                hand.AimAtWorldPoint(hand.Target+offset);for(int i=0;i<65;i++)yield return tick;
                Check(path+" physically rotates",feedback.CurrentAngles.magnitude>5,feedback.CurrentAngles.ToString());
                float limit=path.Contains("Lever")?48:28;Check(path+" respects joint limits",Mathf.Abs(feedback.CurrentAngles.x)<=limit&&Mathf.Abs(feedback.CurrentAngles.y)<=limit,feedback.CurrentAngles.ToString());
                Vector2 releasedAngles=feedback.CurrentAngles;Vector3 anchorBefore=control.transform.TransformPoint(Vector3.down*.25f);
                grab.Release();hand.AimAtWorldPoint(new Vector3(0,1.5f,-.1f));for(int i=0;i<60;i++)yield return tick;
                Check(path+" holds released position",Vector2.Distance(releasedAngles,feedback.CurrentAngles)<5,feedback.CurrentAngles.ToString());
                Check(path+" base remains anchored",Vector3.Distance(anchorBefore,control.transform.TransformPoint(Vector3.down*.25f))<.015f);
                
                Check(path+" retains original constraint",path.Contains("Lever")?control.GetComponent<HingeJoint>()!=null:control.GetComponents<ConfigurableJoint>().Length==1);
            }
            for(int i=0;i<20;i++)look.Step(new Vector2(10,0),true,1f/60);
            float yaw=look.Yaw;float speed=look.AngularVelocity.magnitude;look.Step(Vector2.zero,false,1f/60);
            Check("Look rotates camera pivot",Mathf.Abs(yaw)>1);
            Check("Releasing look preserves momentum",look.AngularVelocity.magnitude>0&&look.AngularVelocity.magnitude<speed&&Mathf.Abs(look.Yaw-yaw)>.01f);
            for(int i=0;i<240;i++)look.Step(Vector2.zero,false,1f/60);
            Check("Momentum decays to rest",look.AngularVelocity.magnitude<.01f);
            for(int i=0;i<120;i++)look.Step(new Vector2(0,100),true,1f/60);
            Check("Minimum pitch limit",Mathf.Abs(look.Pitch+55)<.01f);
            for(int i=0;i<120;i++)look.Step(new Vector2(0,-100),true,1f/60);
            Check("Maximum pitch limit",Mathf.Abs(look.Pitch-60)<.01f);
            Check("No runtime console errors",report.errors==0,report.errors.ToString());
        }
    }
}
#endif
