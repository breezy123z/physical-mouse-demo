using System.Linq;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PhysicalMouseDemo.Editor
{
    public static class DemoUpgrade
    {
        [MenuItem("Tools/Physical Mouse Demo/Apply Hand and Control Upgrade")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)return;
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.name!="PhysicalMouseDemo")return;
            EditorSceneManager.SaveScene(scene,AssetDatabase.GenerateUniqueAssetPath("Assets/PhysicalMouseDemo/Backups/BeforeHandUpgrade.unity"),true);
            var input=Object.FindFirstObjectByType<PlayerInputController>();input.SetMovementBindings();EditorUtility.SetDirty(input);
            foreach(var control in Object.FindObjectsByType<ConstrainedInteractable>(FindObjectsSortMode.None))
            {
                var g=control.GetComponent<Grabbable>();bool lever=control.GetComponent<HingeJoint>();
                g.ConfigureMovement(control.transform.parent,Vector3.right,lever?Vector3.zero:Vector3.forward);
                var rb=control.GetComponent<Rigidbody>();rb.solverIterations=32;rb.solverVelocityIterations=16;rb.angularDamping=3;
                var baseObject=control.transform.parent.Find("Base").gameObject;var baseBody=baseObject.GetComponent<Rigidbody>();if(!baseBody)baseBody=baseObject.AddComponent<Rigidbody>();baseBody.isKinematic=true;baseBody.useGravity=false;
                var constraint=control.GetComponent<Joint>();constraint.connectedBody=baseBody;constraint.autoConfigureConnectedAnchor=false;constraint.connectedAnchor=Vector3.zero;constraint.enableCollision=false;EditorUtility.SetDirty(constraint);
                var joint=control.GetComponent<ConfigurableJoint>();
                if(joint) { joint.projectionMode=JointProjectionMode.PositionAndRotation;joint.projectionDistance=.005f;joint.projectionAngle=2; }
                EditorUtility.SetDirty(g);EditorUtility.SetDirty(rb);if(joint)EditorUtility.SetDirty(joint);
            }
            const string asset="Assets/PhysicalMouseDemo/Hand/Hand_AllAnimations.fbx";
            var importer=(ModelImporter)AssetImporter.GetAtPath(asset);importer.animationType=ModelImporterAnimationType.Generic;importer.importAnimation=true;importer.SaveAndReimport();
            var hand=Object.FindFirstObjectByType<PhysicalHandController>();hand.ConfigureViewReach();
            var edge=input.GetComponent<EdgeTurnController>();if(!edge)edge=input.gameObject.AddComponent<EdgeTurnController>();edge.Configure(input,hand,Camera.main);EditorUtility.SetDirty(edge);EditorUtility.SetDirty(hand);
            var old=hand.transform.Find("HandVisual");if(old)Object.DestroyImmediate(old.gameObject);
            var wrapper=new GameObject("HandVisual");wrapper.transform.SetParent(hand.transform,false);wrapper.transform.localScale=Vector3.one/hand.transform.lossyScale.x;
            var model=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(asset),wrapper.transform);
            var bones=model.GetComponentsInChildren<Transform>();
            Transform wrist=bones.First(x=>x.name=="Wrist"),index=bones.First(x=>x.name=="Index.01"),pinky=bones.First(x=>x.name=="Pinky.01");
            Vector3 forward=((index.position+pinky.position)*.5f-wrist.position).normalized;
            Vector3 side=(index.position-pinky.position).normalized;
            model.transform.rotation=Quaternion.Inverse(Quaternion.LookRotation(forward,Vector3.Cross(forward,side)));
            var renderers=model.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            model.transform.localScale*=.32f/Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);
            bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
            model.transform.position+=hand.transform.position-bounds.center;
            var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/PhysicalMouseDemo/Materials/Hand.mat");foreach(var r in renderers)r.sharedMaterials=Enumerable.Repeat(mat,r.sharedMaterials.Length).ToArray();
            var animator=model.GetComponent<Animator>();if(!animator)animator=model.AddComponent<Animator>();animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var clips=AssetDatabase.LoadAllAssetsAtPath(asset).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
            var idle=clips.First(c=>c.name.EndsWith("Hand_Idle"));var grip=clips.First(c=>c.name.EndsWith("Hand_Grab_Hold"));
            wrapper.AddComponent<HandVisual>().Configure(hand.GetComponent<GrabInteractor>(),animator,idle,grip);
            hand.GetComponent<Renderer>().enabled=false;
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Logs");File.WriteAllText("Logs/HandUpgrade.txt","Hand renderers: "+renderers.Length+"; clips: "+string.Join(", ",clips.Select(c=>c.name)));
            Debug.Log("Hand and control upgrade saved.");
        }
    }
}

