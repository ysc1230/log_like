using UnityEngine;

namespace Survivor2D.Progression
{
    public class LevelSystemLink : MonoBehaviour
    {
        [SerializeField] private LevelSystem levelSystem;
        public LevelSystem LevelSystem => levelSystem;
    }
}
