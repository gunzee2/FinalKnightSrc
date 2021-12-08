using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using MessagePipe;
using UniRx;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public struct SoundEvent
    {
        public enum SoundName
        {
            None,
            SwingNormal,
            SwingHeavy,
            HitNormal,
            HitHeavy,
            BlockStart,
            BlockSuccess,
            FallDownGround,
            ChargeAttackGroundImpact,
            BarrelBreak,
            MegaCrash,
            ChargeStart,
            Tired,
            ItemDrop,
            FrictionMetal
        }

        public enum SoundType
        {
            None,
            Play,
            Stop
        }
        
        public SoundName Name;
        public SoundType Type;
    }
    
    [SerializeField] private string swingNormalSoundName;
    [SerializeField] private string swingHeavySoundName;
    [SerializeField] private string hitNormalSoundName;
    [SerializeField] private string hitHeavySoundName;
    [SerializeField] private string blockStartSoundName;
    [SerializeField] private string blockSuccessSoundName;
    [SerializeField] private string barrelBreakSoundName;
    [SerializeField] private string fallDownSoundName;
    [SerializeField] private string chargeAttackGroundImpactSoundName;
    [SerializeField] private string megaCrashSoundName;
    [SerializeField] private string chargeStartSoundName;
    [SerializeField] private string tiredSoundName;
    [SerializeField] private string itemDropSoundName;
    [SerializeField] private string frictionMetalSoundName;

    private ISubscriber<SoundEvent> _subscriber;
    // Start is called before the first frame update
    void Start()
    {
        _subscriber = GlobalMessagePipe.GetSubscriber<SoundEvent>();
        
        _subscriber.Subscribe(x =>
        {
            var soundName = "";
            switch (x.Name)
            {
                case SoundEvent.SoundName.SwingNormal:
                    soundName = swingNormalSoundName;
                    break;
                case SoundEvent.SoundName.SwingHeavy:
                    soundName = swingHeavySoundName;
                    break;
                case SoundEvent.SoundName.HitNormal:
                    soundName = hitNormalSoundName;
                    break;
                case SoundEvent.SoundName.HitHeavy:
                    soundName = hitHeavySoundName;
                    break;
                case SoundEvent.SoundName.BlockStart:
                    soundName = blockStartSoundName;
                    break;
                case SoundEvent.SoundName.BlockSuccess:
                    soundName = blockSuccessSoundName;
                    break;
                case SoundEvent.SoundName.BarrelBreak:
                    soundName = barrelBreakSoundName;
                    break;
                case SoundEvent.SoundName.FallDownGround:
                    soundName = fallDownSoundName;
                    break;
                case SoundEvent.SoundName.ChargeAttackGroundImpact:
                    soundName = chargeAttackGroundImpactSoundName;
                    break;
                case SoundEvent.SoundName.ChargeStart:
                    soundName = chargeStartSoundName;
                    break;
                case SoundEvent.SoundName.Tired:
                    soundName = tiredSoundName;
                    break;
                case SoundEvent.SoundName.MegaCrash:
                    soundName = megaCrashSoundName;
                    break;
                case SoundEvent.SoundName.ItemDrop:
                    soundName = itemDropSoundName;
                    break;
                case SoundEvent.SoundName.FrictionMetal:
                    soundName = frictionMetalSoundName;
                    break;
                default:
                    return;
            }

            switch (x.Type)
            {
                case SoundEvent.SoundType.Play:
                    MasterAudio.PlaySound(soundName);
                    break;
                case SoundEvent.SoundType.Stop:
                    MasterAudio.StopAllOfSound(soundName);
                    break;
            }


        }).AddTo(this);

    }

}