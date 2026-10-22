namespace ShorelineDefense.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(int amount);
    }
}
