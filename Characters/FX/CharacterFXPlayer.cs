using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityTransform;
using Characters.Actions;
using Chronos;
using UniRx;
using UnityEngine;

namespace Characters.FX
{
    public class CharacterFXPlayer : MonoBehaviour
    {
        public enum FXType
        {
            None,
            Combo1,
            Combo2,
            Combo3,
            Jump1,
            Jump2,
            ChargeAttack,
            BlockStart,
            BlockSuccess,
            ChargeAttackGroundImpact,
            HitNormal,
            HitHeavy,
            FallDownGround,
            FootSmoke,
            MegaCrash,
            ChargeStart,
            Tired,
            ItemDrop,
            SuperArmorStart
        }
        
        [SerializeField] private ParticleSystem footSmokeParticleSystem;
        [SerializeField] private ParticleSystem weaponSlashParticleSystem;
        [SerializeField] private ParticleSystem chargeAttackGroundImpactParticleSystem;
        [SerializeField] private ParticleSystem megaCrashParticleSystem;
        [SerializeField] private ParticleSystem hitEffectParticleSystem;
        [SerializeField] private ParticleSystem downSmokeParticleSystem;
        
        

        private ICharacterEventProvider _characterEventProvider;
        private ICharacterStateProvider _characterStateProvider;
        private ICharacterCollisionEventProvider _characterCollisionEventProvider;

        private Timeline _timeline;
        
        
        [SerializeField] private Vector3 combo1FXPosition;
        [SerializeField] private Vector3 combo2FXPosition;
        [SerializeField] private Vector3 combo3FXPosition;
        [SerializeField] private Vector3 jump1FXPosition;
        [SerializeField] private Vector3 jump2FXPosition;

        [SerializeField] private Vector3 combo1FXRotation;
        [SerializeField] private Vector3 combo2FXRotation;
        [SerializeField] private Vector3 combo3FXRotation;
        [SerializeField] private Vector3 jump1FXRotation;
        [SerializeField] private Vector3 jump2FXRotation;
        [SerializeField] private Vector3 chargeAttackFXRotation;
        private Vector3 combo1Rotation;
        
        
        void Start()
        {
            _characterEventProvider = GetComponent<ICharacterEventProvider>();
            _characterStateProvider = GetComponent<ICharacterStateProvider>();
            _characterCollisionEventProvider = GetComponent<ICharacterCollisionEventProvider>();

            _timeline = GetComponent<Timeline>();
            if(weaponSlashParticleSystem)
                _timeline.SetParticleSystem(weaponSlashParticleSystem);

            _characterEventProvider.OnKnockBack.Where(x => !x.IsBlowAway).Subscribe(x =>
            {
                if(!footSmokeParticleSystem.isPlaying)
                    footSmokeParticleSystem.Play();
            }).AddTo(this);
            
            _characterStateProvider.CurrentActionState
                .Where(x => x != ActionState.KnockBack)
                .Subscribe(x =>
                {
                    if(footSmokeParticleSystem.isPlaying)
                        footSmokeParticleSystem.Stop();
                }).AddTo(this);
            
            _characterStateProvider.CurrentActionState
                .Pairwise()
                .Where(x => x.Previous == ActionState.BlockSuccess)
                .Where(x => x.Current != ActionState.BlockSuccess)
                .Subscribe(x =>
                {
                    if(footSmokeParticleSystem.isPlaying)
                        footSmokeParticleSystem.Stop();
                }).AddTo(this);

            _characterEventProvider.OnFallDownGround.Subscribe(_ =>
            {
                EmitParticleSystem(downSmokeParticleSystem, transform.position, 20, 24);
            }).AddTo(this);

            _characterCollisionEventProvider.OnAttackCollisionHit.Subscribe(x =>
            {
                EmitParticleSystem(hitEffectParticleSystem, x.HitPosition, 100, 200);
            }).AddTo(this);

            _characterStateProvider.CurrentActionState
                .Where(x => x != ActionState.GroundAttack && x != ActionState.WaitNextAttack).Subscribe(x =>
                {
                    if (!weaponSlashParticleSystem) return;
                    
                    Debug.Log("Stop Weapon Fx", gameObject);
                    if (_timeline.particleSystem.isPlaying)
                    {
                        Debug.Log("Particle System is Playing. To Stop.", gameObject);
                        _timeline.particleSystem.Clear();
                        _timeline.particleSystem.Stop();
                    }
                    chargeAttackGroundImpactParticleSystem.Stop();
                }).AddTo(this);

        }

        public void PlayFx(FXType type)
        {
            if (_timeline.particleSystem.isPlaying)
            {
                _timeline.particleSystem.Clear();
                _timeline.particleSystem.Stop();
            }
            switch (type)
            {
                case FXType.Combo1:
                    PlayWeaponAttackFx(combo1FXPosition, combo1FXRotation);
                    break;
                case FXType.Combo2:
                    PlayWeaponAttackFx(combo2FXPosition, combo2FXRotation);
                    break;
                case FXType.Combo3:
                    PlayWeaponAttackFx(combo3FXPosition, combo3FXRotation);
                    break;
                case FXType.ChargeAttack:
                    PlayWeaponAttackFx(combo1FXPosition, chargeAttackFXRotation);
                    break;
                case FXType.Jump1:
                    PlayWeaponAttackFx(jump1FXPosition, jump1FXRotation);
                    break;
                case FXType.Jump2:
                    PlayWeaponAttackFx(jump2FXPosition, jump2FXRotation);
                    break;
                case FXType.ChargeAttackGroundImpact:
                    PlayParticleSystem(chargeAttackGroundImpactParticleSystem);
                    break;
                case FXType.FootSmoke:
                    PlayParticleSystem(footSmokeParticleSystem);
                    break;
                case FXType.MegaCrash:
                    PlayParticleSystem(megaCrashParticleSystem);
                    break;
            }
        }

        private void EmitParticleSystem(ParticleSystem ps, Vector3 pos, int emitMin, int emitMax)
        {
            Debug.Log("HitEffect Pos => " + pos, gameObject);
            ps.transform.position = pos;
            ps.Emit(Random.Range(emitMin,emitMax));
        }

        private void PlayParticleSystem(ParticleSystem ps)
        {
            if (ps.isPlaying)
            {
                
                ps.Clear();
                ps.Stop();
            }
            ps.Play();
        }

        private void PlayWeaponAttackFx(Vector3 pos, Vector3 eulerAngles)
        {
            if (!weaponSlashParticleSystem) return;
            if (_characterStateProvider.CurrentActionState.Value != ActionState.GroundAttack &&
                _characterStateProvider.CurrentActionState.Value != ActionState.WaitNextAttack &&
                _characterStateProvider.CurrentActionState.Value != ActionState.JumpAttack
            ) return;

            if (_timeline.particleSystem.isPlaying)
            {
                Debug.Log("Particle System is Playing. To Stop.", gameObject);
                _timeline.particleSystem.Clear();
                _timeline.particleSystem.Stop();
            }
            
            
            var particleTransform = weaponSlashParticleSystem.transform;
            particleTransform.localPosition = pos;
            particleTransform.localEulerAngles = eulerAngles;

            _timeline.particleSystem.Play();
        }
    }
}