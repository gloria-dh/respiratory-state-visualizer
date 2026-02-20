namespace Respiratory_State_Visualizer_V0
{
    internal class AvatarState
    {
        internal RespiratoryState DisplayState { get; set; }
    }

    internal enum RespiratoryState { Calm, HoldingBreath, Hyperventilating }
}
