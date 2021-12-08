using Characters.Actions;

namespace Characters.Damages
{
    public struct CameraShakeEventData
    {
        public enum CameraShakeType
        {
            None,
            Light,
            Heavy
        }

        public CameraShakeType ShakeType; 
        
        public CameraShakeEventData(CameraShakeType type)
        {
            ShakeType = type;
        }
    }
}
