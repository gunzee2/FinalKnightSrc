using System;
using Characters.Actions;
using Characters.FX;
using DarkTonic.MasterAudio;
using MessagePipe;
using UniRx;
using UnityEngine;

namespace Characters.Sounds
{
    public class CharacterSoundPlayer : MonoBehaviour
    {

        private ICharacterStateProvider _characterStateProvider;
        private ICharacterEventProvider _characterEventProvider;
        private IAttackEventProvider _attackEventProvider;
        private ICharacterCollisionEventProvider _characterCollisionEventProvider;

        private IPublisher<SoundManager.SoundEvent> _publisher;

        private void Awake()
        {
            _characterEventProvider = GetComponent<ICharacterEventProvider>();
            _characterStateProvider = GetComponent<ICharacterStateProvider>();
            _attackEventProvider = GetComponent<IAttackEventProvider>();
            _characterCollisionEventProvider = GetComponent<ICharacterCollisionEventProvider>();
        }

        void Start()
        {
            _publisher = GlobalMessagePipe.GetPublisher<SoundManager.SoundEvent>();

            _characterEventProvider.OnDamaged.Subscribe(x =>
            {
                switch (x.attackType)
                {
                    case AttackType.ChargeAttack:
                        PlaySoundFX(CharacterFXPlayer.FXType.HitHeavy);
                        break;
                    default:
                        PlaySoundFX(CharacterFXPlayer.FXType.HitNormal);
                        break;
                }
            }).AddTo(this);

            _characterEventProvider.OnFallDownGround.Subscribe(_ =>
            {
                PlaySoundFX(CharacterFXPlayer.FXType.FallDownGround);
            }).AddTo(this);

            _characterEventProvider.OnBlockSuccessful.Subscribe(x =>
            {
                PlaySoundFX(CharacterFXPlayer.FXType.BlockSuccess);
            }).AddTo(this);

            _characterStateProvider.CurrentActionState.Subscribe(x =>
            {
                switch (x)
                {
                    case ActionState.Block:
                        PlaySoundFX(CharacterFXPlayer.FXType.BlockStart);
                        break;
                }
            }).AddTo(this);

            _attackEventProvider.OnChargeStart.Subscribe(x => { PlaySoundFX(CharacterFXPlayer.FXType.ChargeStart); })
                .AddTo(this);
            _attackEventProvider.OnChargeEnd.Subscribe(x => { StopSoundFX(CharacterFXPlayer.FXType.ChargeStart); })
                .AddTo(this);
            _characterStateProvider.CurrentActionState.Where(x => x == ActionState.Tired).Subscribe(_ =>
            {
                PlaySoundFX(CharacterFXPlayer.FXType.Tired);
            }).AddTo(this);
            _characterStateProvider.CurrentActionState
                .Pairwise()
                .Where(x => x.Previous == ActionState.Tired)
                .Where(x => x.Current != ActionState.Tired)
                .Subscribe(_ =>
            {
                StopSoundFX(CharacterFXPlayer.FXType.Tired);
            }).AddTo(this);

            _characterCollisionEventProvider.OnThrowItemCollisionDrop.Subscribe(_ =>
            {
                PlaySoundFX(CharacterFXPlayer.FXType.ItemDrop);
            }).AddTo(this);
        }

        public void PlaySoundFX(CharacterFXPlayer.FXType type)
        {
            var ev = SelectSoundName(type);

            Debug.Log(ev.ToString(),gameObject);
            var soundEvent = new SoundManager.SoundEvent {Name = ev, Type = SoundManager.SoundEvent.SoundType.Play};

            _publisher.Publish(soundEvent);
        }

        public void StopSoundFX(CharacterFXPlayer.FXType type)
        {
            var ev = SelectSoundName(type);

            var soundEvent = new SoundManager.SoundEvent {Name = ev, Type = SoundManager.SoundEvent.SoundType.Stop};

            _publisher.Publish(soundEvent);
            
        }

        public SoundManager.SoundEvent.SoundName SelectSoundName(CharacterFXPlayer.FXType type)
        {
            SoundManager.SoundEvent.SoundName soundName = SoundManager.SoundEvent.SoundName.None;
            switch (type)
            {
                case CharacterFXPlayer.FXType.Combo1:
                    soundName = SoundManager.SoundEvent.SoundName.SwingNormal;
                    break;
                case CharacterFXPlayer.FXType.Combo2:
                    soundName = SoundManager.SoundEvent.SoundName.SwingNormal;
                    break;
                case CharacterFXPlayer.FXType.Combo3:
                    soundName = SoundManager.SoundEvent.SoundName.SwingHeavy;
                    break;
                case CharacterFXPlayer.FXType.Jump1:
                    soundName = SoundManager.SoundEvent.SoundName.SwingNormal;
                    break;
                case CharacterFXPlayer.FXType.Jump2:
                    soundName = SoundManager.SoundEvent.SoundName.SwingNormal;
                    break;
                case CharacterFXPlayer.FXType.ChargeAttack:
                    soundName = SoundManager.SoundEvent.SoundName.SwingHeavy;
                    break;
                case CharacterFXPlayer.FXType.BlockStart:
                    soundName = SoundManager.SoundEvent.SoundName.BlockStart;
                    break;
                case CharacterFXPlayer.FXType.BlockSuccess:
                    soundName = SoundManager.SoundEvent.SoundName.BlockSuccess;
                    break;
                case CharacterFXPlayer.FXType.ChargeAttackGroundImpact:
                    soundName = SoundManager.SoundEvent.SoundName.ChargeAttackGroundImpact;
                    break;
                case CharacterFXPlayer.FXType.HitNormal:
                    soundName = SoundManager.SoundEvent.SoundName.HitNormal;
                    break;
                case CharacterFXPlayer.FXType.HitHeavy:
                    soundName = SoundManager.SoundEvent.SoundName.HitHeavy;
                    break;
                case CharacterFXPlayer.FXType.FallDownGround:
                    soundName = SoundManager.SoundEvent.SoundName.FallDownGround;
                    break;
                case CharacterFXPlayer.FXType.ChargeStart:
                    soundName = SoundManager.SoundEvent.SoundName.ChargeStart;
                    break;
                case CharacterFXPlayer.FXType.Tired:
                    soundName = SoundManager.SoundEvent.SoundName.Tired;
                    break;
                case CharacterFXPlayer.FXType.MegaCrash:
                    soundName = SoundManager.SoundEvent.SoundName.MegaCrash;
                    break;
                case CharacterFXPlayer.FXType.ItemDrop:
                    soundName = SoundManager.SoundEvent.SoundName.ItemDrop;
                    break;
                case CharacterFXPlayer.FXType.SuperArmorStart:
                    soundName = SoundManager.SoundEvent.SoundName.FrictionMetal;
                    break;
            }
            return soundName;
        }
    }
}