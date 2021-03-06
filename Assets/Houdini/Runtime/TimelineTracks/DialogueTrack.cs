using UnityEngine;
namespace CinemaDirector
{
    [TimelineTrackAttribute("Dialogue Track", TimelineTrackGenre.CharacterTrack, CutsceneItemGenre.AudioClipItem)]
    public class DialogueTrack : AudioTrack, IActorTrack
    {
        
        [SerializeField]
        private Transform anchor = null;

        public override void Initialize()
        {
            base.Initialize();
            setTransform();
        }

        public override void UpdateTrack(float time, float deltaTime)
        {
            setTransform();
            base.UpdateTrack(time, deltaTime);
        }

        private void setTransform()
        {
            if (anchor != null)
            {
                transform.position = anchor.position;
            }
            else if (Actor != null)
            {
                transform.position = Actor.transform.position;
            }
        }

        public Transform Actor
        {
            get
            {
                ActorTrackGroup component = transform.parent.GetComponent<ActorTrackGroup>();
                if (component == null)
                {
                    return null;
                }
                return component.Actor;
            }
        }
    }
}