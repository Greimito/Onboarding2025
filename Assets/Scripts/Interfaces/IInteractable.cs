using UnityEngine;

namespace Platformer25D
{
    /// <summary>
    /// Interfaz para objetos con los que el jugador o proyectiles pueden interactuar.
    /// </summary>
    public interface IInteractable
    {
        void Interact(GameObject source);
    }

    /// <summary>
    /// Alias opcional para compatibilidad con la nomenclatura IInteract.
    /// </summary>
    public interface IInteract : IInteractable
    {
    }
}
