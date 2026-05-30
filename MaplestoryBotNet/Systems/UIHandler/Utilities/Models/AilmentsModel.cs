using System.Collections.Concurrent;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities.Models
{
    public abstract class AbstractAilmentsModel
    {
        public abstract void SetAilmentsModel(AbstractAilmentsModel model);

        public abstract List<Tuple<string, int>> GetAilments();

        public abstract int GetAilment(string ailment);

        public abstract void SetAilment(string ailment, int status);

        public abstract AbstractAilmentsModel Copy();
    }


    public class AilmentsModel : AbstractAilmentsModel
    {
        private ConcurrentDictionary<string, int> _ailmentDetected = [];

        public override AbstractAilmentsModel Copy()
        {
            var ailmentsModel = new AilmentsModel();
            ailmentsModel.SetAilmentsModel(this);
            return ailmentsModel;
        }

        public override int GetAilment(string ailment)
        {
            return _ailmentDetected[ailment];
        }

        public override List<Tuple<string, int>> GetAilments()
        {
            return _ailmentDetected.Select(
                (kv) => new Tuple<string, int>(kv.Key, kv.Value)
            ).ToList();
        }

        public override void SetAilment(string ailment, int status)
        {
            _ailmentDetected[ailment] = status;
        }

        public override void SetAilmentsModel(AbstractAilmentsModel model)
        {
            foreach (var ailment in model.GetAilments())
            {
                _ailmentDetected[ailment.Item1] = ailment.Item2;
            }
        }
    }
}
