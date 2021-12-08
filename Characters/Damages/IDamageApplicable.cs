using System;
using UniRx;

namespace Characters.Damages
{
    public interface IDamageApplicable
    {
        IReadOnlyReactiveProperty<bool> IsInvincible { get; }
        public Guid PreviousDamageId { get; }
        void ApplyDamage(DamageData damageData);
    }

    public interface IMegaCrashDamageApplicable
    {
        void ApplyMegaCrashDamage(int value);
    }
}
