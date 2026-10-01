using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PhysicalMouseDemo.Editor
{
    public static class PhysicalMouseDemoBuilder
    {
        public const string Folder = "Assets/PhysicalMouseDemo";
        public const string ScenePath = Folder + "/PhysicalMouseDemo.unity";
        [MenuItem("Tools/Physical Mouse Demo/Build or Reset Demo Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Stop Play mode before rebuilding."); return; }
            Directory.CreateDirectory(Folder + "/Materials");Directory.CreateDirectory(Folder + "/Backups");
            var demo = SceneManager.GetSceneByPath(ScenePath);
            if (!demo.isLoaded) demo = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(demo);
            // Preserve every other loaded scene (including unsaved edits) as a copy before switching.
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var other = SceneManager.GetSceneAt(i);
                if (other == demo) continue;
                string backup = Folder + "/Backups/" + other.name + "_WorkingCopy.unity";
                backup = AssetDatabase.GenerateUniqueAssetPath(backup);
                EditorSceneManager.SaveScene(other, backup, true);
                EditorSceneManager.CloseScene(other, true);
            }
            foreach (var go in demo.GetRootGameObjects()) Object.DestroyImmediate(go);
            Material ground = Material("Ground", new Color(.12f,.17f,.22f));
            Material dark = Material("Dark", new Color(.22f,.29f,.35f));
            Material cyan = Material("Hand", new Color(.25f,.8f,1f));
            Material green = Material("LightCube", new Color(.2f,.8f,.5f));
            Material orange = Material("HeavyCube", new Color(.95f,.45f,.12f));
            Material blue = Material("FlightStick", new Color(.28f,.5f,.95f));
            Material yellow = Material("Lever", new Color(.95f,.75f,.18f));
            var env = new GameObject("Environment").transform;
            Primitive("Floor", PrimitiveType.Cube, env, new Vector3(0,-.06f,0), new Vector3(8,.12f,8), ground);
            Primitive("Workbench", PrimitiveType.Cube, env, new Vector3(0,.65f,.25f), new Vector3(2.4f,.12f,1.5f), dark);
            Cube("GrabCube", env, new Vector3(-.3f,.84f,.12f), 1f, green);
            Cube("HeavyCube", env, new Vector3(.3f,.84f,.12f), 8f, orange);
            var lever = Control("LeverDemo", env, new Vector3(.8f,.74f,.23f), true, yellow, dark);
            var stick = Control("AircraftControlDemo", env, new Vector3(-.8f,.74f,.23f), false, blue, dark);
            Label("1 kg", env, new Vector3(-.3f,.74f,-.27f), .012f);
            Label("8 kg", env, new Vector3(.3f,.74f,-.27f), .012f);
            Label("LEVER", env, new Vector3(.8f,.74f,-.27f), .012f);
            Label("FLIGHT STICK", env, new Vector3(-.8f,.74f,-.27f), .010f);
            var player = new GameObject("Player");player.transform.position = new Vector3(0,0,-1.8f);
            var input = player.AddComponent<PlayerInputController>();
            var pivot = new GameObject("CameraPivot").transform;pivot.SetParent(player.transform,false);pivot.localPosition = new Vector3(0,1.45f,0);
            var camGo = new GameObject("Main Camera");camGo.transform.SetParent(pivot,false);camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();cam.nearClipPlane = .03f;cam.farClipPlane = 50;cam.fieldOfView = 65;cam.clearFlags = CameraClearFlags.SolidColor;cam.backgroundColor = new Color(.07f,.1f,.14f);camGo.AddComponent<AudioListener>();
            var anchor = new GameObject("HandAnchor").transform;anchor.SetParent(pivot,false);anchor.localPosition = new Vector3(0,-.38f,2);
            var handGo = Primitive("PhysicalHand", PrimitiveType.Sphere, anchor, anchor.position, Vector3.one * .17f, cyan);
            var body = handGo.AddComponent<Rigidbody>();body.mass = .7f;body.useGravity = false;body.constraints = RigidbodyConstraints.FreezeRotation;body.interpolation = RigidbodyInterpolation.Interpolate;body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;body.solverIterations = 12;body.solverVelocityIterations = 8;
            var hand = handGo.AddComponent<PhysicalHandController>();hand.Configure(input,pivot);
            var grab = handGo.AddComponent<GrabInteractor>();grab.Configure(input,hand);
            var look = player.AddComponent<CameraLookController>();look.Configure(input,pivot);
            var feedback = player.AddComponent<DemoFeedback>();feedback.Configure(input,grab,look,lever,stick,handGo.GetComponent<Renderer>());
            var lighting = new GameObject("Lighting").transform;
            var sun = new GameObject("Sun");sun.transform.SetParent(lighting);sun.transform.rotation = Quaternion.Euler(45,-30,0);var light = sun.AddComponent<Light>();light.type = LightType.Directional;light.intensity = 2;light.shadows = LightShadows.Soft;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight = new Color(.55f,.6f,.68f);
            EditorSceneManager.SaveScene(demo,ScenePath);AssetDatabase.SaveAssets();
            DemoUpgrade.Apply();
            Selection.activeGameObject = player;
            if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.LookAt(new Vector3(0,1,.2f),Quaternion.Euler(25,0,0),3f);
            Debug.Log("PhysicalMouseDemo built. Main scene preserved; working-copy backup saved. Press Play to try it.");
        }
        private static Material Material(string name, Color color)
        {
            string path = Folder + "/Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path); }
            mat.SetColor("_BaseColor",color);mat.SetFloat("_Smoothness",.28f);return mat;
        }
        private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 position, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);go.name = name;go.transform.SetParent(parent,true);go.transform.position = position;go.transform.localScale = scale;go.GetComponent<Renderer>().sharedMaterial = mat;return go;
        }
        private static void Cube(string name, Transform parent, Vector3 position, float mass, Material mat)
        {
            var go = Primitive(name,PrimitiveType.Cube,parent,position,Vector3.one*.23f,mat);var rb = go.AddComponent<Rigidbody>();rb.mass = mass;rb.interpolation = RigidbodyInterpolation.Interpolate;rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;rb.solverIterations = 12;rb.solverVelocityIterations = 8;go.AddComponent<Grabbable>().SetName(name + " (" + mass + " kg)");
        }
        private static ConstrainedInteractable Control(string name, Transform parent, Vector3 origin, bool lever, Material mat, Material dark)
        {
            var root = new GameObject(name).transform;root.SetParent(parent);root.position = origin;
            Primitive("Base",PrimitiveType.Cube,root,origin, new Vector3(.28f,.06f,.28f),dark);
            var go = new GameObject("Handle");go.transform.SetParent(root);go.transform.localPosition = new Vector3(0,.25f,0);
            Primitive("Shaft",PrimitiveType.Cube,go.transform,go.transform.position,new Vector3(.06f,.5f,.06f),dark);
            Primitive("Grip",PrimitiveType.Sphere,go.transform,go.transform.position+Vector3.up*.25f,Vector3.one*.18f,mat);
            var rb = go.AddComponent<Rigidbody>();rb.mass = 1.5f;rb.useGravity = false;rb.interpolation = RigidbodyInterpolation.Interpolate;rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;rb.solverIterations = 16;rb.solverVelocityIterations = 8;
            go.AddComponent<Grabbable>().SetName(lever ? "Lever" : "Flight stick");
            if (lever)
            {
                var joint = go.AddComponent<HingeJoint>();joint.anchor = Vector3.down*.25f;joint.axis = Vector3.forward;joint.useLimits = true;joint.limits = new JointLimits { min=-45,max=45 };joint.useSpring = true;joint.spring = new JointSpring { spring=10,damper=2,targetPosition=0 };
            }
            else
            {
                var joint = go.AddComponent<ConfigurableJoint>();joint.anchor = Vector3.down*.25f;joint.autoConfigureConnectedAnchor = false;joint.connectedAnchor = origin;
                joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Locked;joint.angularXMotion = ConfigurableJointMotion.Limited;joint.angularYMotion = ConfigurableJointMotion.Locked;joint.angularZMotion = ConfigurableJointMotion.Limited;
                joint.lowAngularXLimit = new SoftJointLimit { limit=-25 };joint.highAngularXLimit = new SoftJointLimit { limit=25 };joint.angularZLimit = new SoftJointLimit { limit=25 };
                joint.rotationDriveMode = RotationDriveMode.XYAndZ;joint.angularXDrive = joint.angularYZDrive = new JointDrive { positionSpring=12,positionDamper=2,maximumForce=40 };
            }
            var control = go.AddComponent<ConstrainedInteractable>();control.Configure(lever ? ConstrainedInteractable.ControlType.Lever : ConstrainedInteractable.ControlType.FlightStick,lever ? 45:25);return control;
        }
        private static void Label(string text, Transform parent, Vector3 position, float size)
        {
            var go = new GameObject(text);go.transform.SetParent(parent);go.transform.position = position;var t = go.AddComponent<TextMesh>();t.text = text;t.anchor = TextAnchor.MiddleCenter;t.alignment = TextAlignment.Center;t.characterSize = size;t.fontSize = 48;t.color = Color.white;go.transform.rotation = Quaternion.Euler(60,0,0);
        }
    }
}

