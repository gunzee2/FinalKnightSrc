using Characters.Actions;
using UniRx;
using UnityEngine;

namespace Utility.Editor
{
    [UnityEditor.CustomPropertyDrawer(typeof(ActionStateReactiveProperty))]
    public class ExtendInspectorDisplayDrawer : InspectorDisplayDrawer
    {
    }
}
