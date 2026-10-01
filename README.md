# Onboarding 2025 - Evaluación Técnica y Creativa en Unity 2.5D

[![Unity](https://img.shields.io/badge/Unity-6000.5.3f1%20(Unity%206)-blue.svg?logo=unity)](https://unity.com/)
[![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP%2017.5-lightgrey.svg)](https://unity.com/srp/Universal-Render-Pipeline)
[![C#](https://img.shields.io/badge/Language-C%23-239120.svg?logo=csharp)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Git LFS](https://img.shields.io/badge/Git-LFS%20Enabled-orange.svg?logo=git-lfs)](https://git-lfs.github.com/)

---

## 📌 Descripción del Proyecto

Este repositorio contiene un **proyecto de formación y evaluación técnica**, diseñado para valorar la **capacidad creativa dentro del entorno de Unity** y consolidar conocimientos fundamentales y buenas prácticas sobre:

- **Físicas y Cinemática 3D en Entornos 2.5D**: Simulación de gravedad, control de inercia y colisiones en planos inclinados.
- **Sistemas de Spawning y Escalado de Dificultad**: Generación procedimental de proyectiles con temporizadores adaptativos según el tiempo de partida.
- **Arquitectura Basada en Interfaces**: Desacoplamiento de componentes mediante contratos limpios (`IDamageable`, `IInteractable`).
- **Game Management & Game Feel**: Control del ciclo de vida de la partida, invulnerabilidad temporal (*i-frames*), *Coyote Time* y *Jump Buffer*.

---

## 🏗️ Arquitectura y Sistemas Principales

### 1. Controlador de Personaje 2.5D (`SphereController25D`)
- **Movimiento Lateral y Restricción 2.5D**: Movimiento fluido sobre el eje $X$ con bloqueo en el eje $Z$.
- **Mecánicas de Salto Avanzadas**:
  - *Coyote Time* (0.15s) para permitir saltos tras abandonar una plataforma.
  - *Jump Buffer* (0.12s) para registrar entradas previas al contacto con el suelo.
  - Gravedad adaptativa (caída rápida y salto corto al soltar el botón).
  - Soporte para saltos múltiples (doble salto configurable).
- **Rodamiento Visual**: Cálculo en tiempo real del ángulo de rotación a partir del radio y la velocidad lineal para un efecto visual realista sin alterar las restricciones físicas del `Rigidbody`.
- **Doble Compatibilidad de Entrada**: Soporte nativo para el *New Input System* y fallback automático al *Legacy Input Manager*.

### 2. Sistema de Spawning Dinámico (`HazardSpawner`)
- Array de puntos de aparición (`Transform[]`) distribuidos en el escenario.
- Algoritmo de selección con prevención de repetición inmediata en el mismo punto.
- **Curva de Dificultad Progresiva**: El intervalo de aparición disminuye de forma continua desde un valor inicial (`initialSpawnInterval = 3.0s`) hasta un valor límite (`minSpawnInterval = 0.5s`) según el tiempo sobrevivido.

### 3. Físicas de Proyectiles y Peligros de Zona (`HazardBall` & `HazardZone`)
- **Comportamiento Físico Natural**: Las bolas enemigas caen y ruedan por gravedad pura sobre superficies y planos inclinados sin bloqueos artificiales de ejes.
- **Impacto Directo**: Si golpea una entidad con `IDamageable`, inflige daño directo.
- **Generación de Zonal al Contacto con el Suelo**:
  - Se destruye e instancia un área dañina (`HazardZone`) alineada a la **normal de la superficie**.
  - **Duración de Vida Serializada**: Configurada por defecto a **5 segundos** (`lifetime = 5.0s`), tras lo cual se autodestruye.
  - Animación de advertencia visual (encogimiento/pulso) antes de desaparecer.

### 4. Interfaces y Desacoplamiento (`IDamageable`, `IInteractable`)
- **`IDamageable` / `IDamage`**: Define el método `TakeDamage(int damage)`, permitiendo que cualquier objeto (jugador, enemigos, destructibles) reciba daño sin acoplamiento directo entre clases.
- **`IInteractable` / `IInteract`**: Define el contrato `Interact(GameObject source)` para desencadenar interacciones contextuales.

### 5. Control de Partida e Interfaz (`GameManager`, `PlayerHealth`, `GameHUD`)
- **`GameManager`**: Patrón Singleton que coordina el estado de la partida (`Playing`, `GameOver`), el cronómetro global y la persistencia de salud.
- **`PlayerHealth`**: Implementa `IDamageable`, gestiona la barra de salud, los cuadros de invulnerabilidad post-daño con parpadeo del *Renderer* y la secuencia de derrota.
- **`GameHUD`**: Interfaz de usuario inmediata (OnGUI / Canvas) con contador de tiempo, visualización de corazones de vida y pantalla de reinicio rápido.

---

## 📂 Estructura del Código

```text
Assets/Scripts/
├── Interfaces/
│   ├── IDamageable.cs          # Contrato para entidades que reciben daño
│   └── IInteractable.cs        # Contrato para interacciones
├── SphereController25D.cs      # Controlador 2.5D de la esfera con física y Game Feel
├── CameraFollow25D.cs          # Cámara suave con límites y seguimiento 2.5D
├── PlayerHealth.cs             # Gestión de vida, i-frames y feedback visual
├── HazardSpawner.cs            # Spawner modular con dificultad progresiva
├── HazardBall.cs               # Bola con física libre sobre planos inclinados
├── HazardZone.cs               # Zona dañina temporal con alineación a la normal
├── GameManager.cs              # Singleton de control de partida y cronómetro
└── GameHUD.cs                  # Interfaz gráfica en pantalla y menú de Game Over
```

---

## 🎮 Controles por Defecto

| Acción | Teclado | Gamepad |
|---|---|---|
| **Mover Izquierda / Derecha** | `A` / `D` o `←` / `→` | *Left Stick* / *D-Pad* |
| **Saltar** | `Espacio` / `W` / `↑` | Botón Sur (`A` en Xbox / `X` en PS) |
| **Reiniciar (Game Over)** | Botón de interfaz | Botón de interfaz |

---

## ⚙️ Requisitos y Tecnologías

- **Motor**: Unity 6 (6000.5.3f1) o superior.
- **Render Pipeline**: Universal Render Pipeline (URP).
- **Control de Versiones**: Git con soporte para **Git LFS** activado (`.gitattributes`).

---

## 🚀 Instalación y Clonado

Para clonar este repositorio asegurando la correcta descarga de archivos binarios vía LFS:

```bash
# 1. Asegurar la instalación de Git LFS
git lfs install

# 2. Clonar el repositorio
git clone <URL_DEL_REPOSITORIO>

# 3. Abrir el proyecto en Unity Hub utilizando la versión de Unity 6 indicada.
```

---
*Proyecto desarrollado con fines formativos y de evaluación técnica.*
