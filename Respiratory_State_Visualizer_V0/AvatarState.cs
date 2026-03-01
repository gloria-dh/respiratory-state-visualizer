namespace Respiratory_State_Visualizer_V0
{
    internal class AvatarState
    {
        internal RespiratoryState DisplayState { get; set; }
        internal float HeartRate { get; set; }
        internal float BreathRate { get; set; }
        internal float BreathDeviation { get; set; }
    }

    internal enum RespiratoryState { Neutral, Strained, HoldingBreath, Recovering, Alert }
}
