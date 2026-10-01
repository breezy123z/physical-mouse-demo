using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace PhysicalMouseDemo
{
    // Animation affects only the visual child; physics stays on the parent Rigidbody.
    public sealed class HandVisual : MonoBehaviour
    {
        [SerializeField] private GrabInteractor grab;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip idle, grip;
        [SerializeField, Min(.01f)] private float blendTime=.18f;
        private PlayableGraph graph;
        private AnimationMixerPlayable mixer;
        private float blend;
        public float GripBlend => blend;
        public void Configure(GrabInteractor source, Animator target, AnimationClip relaxed, AnimationClip closed)
        { grab=source;animator=target;idle=relaxed;grip=closed; }
        private void OnEnable()
        {
            if(!animator || !idle || !grip)return;
            animator.applyRootMotion=false;
            graph=PlayableGraph.Create("Physical hand poses");
            mixer=AnimationMixerPlayable.Create(graph,2);
            var a=AnimationClipPlayable.Create(graph,idle);var b=AnimationClipPlayable.Create(graph,grip);
            graph.Connect(a,0,mixer,0);graph.Connect(b,0,mixer,1);
            a.SetSpeed(0);b.SetSpeed(0);mixer.SetInputWeight(0,1);
            AnimationPlayableOutput.Create(graph,"Hand",animator).SetSourcePlayable(mixer);graph.Play();
        }
        private void Update()
        {
            if(!graph.IsValid())return;
            blend=Mathf.MoveTowards(blend,grab && grab.IsHolding ? 1:0,Time.deltaTime/blendTime);
            mixer.SetInputWeight(0,1-blend);mixer.SetInputWeight(1,blend);
        }
        private void OnDisable() { if(graph.IsValid())graph.Destroy(); }
    }
}
