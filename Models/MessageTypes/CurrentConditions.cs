namespace Windsock
{
    public class CurrentConditions
    {
        private TWC_CurrentObservation currentObservation;

        public CurrentConditions(TWC_CurrentObservation observation)
        {
            currentObservation = observation;
        }

        public List<string> Serialize()
        {
            return new List<string>();
        }
    }
}