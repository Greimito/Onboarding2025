using UnityEngine;

namespace Platformer25D
{
    /// <summary>
    /// Interfaz para cualquier entidad que pueda recibir daño (Jugador, Enemigos, Objetos destructibles).
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int damage);
    }

    /// <summary>
    /// Alias opcional para compatibilidad directa con la nomenclatura IDamage.
    /// </summary>
    public interface IDamage : IDamageable
    {
    }
}
