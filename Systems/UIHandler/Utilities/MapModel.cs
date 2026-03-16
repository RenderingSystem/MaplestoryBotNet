using MaplestoryBotNet.Systems.Configuration.SubSystems;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Input;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public class MinimapPointMacros
    {
        public string MacroName = "";

        public int MacroChance = 0;

        public List<string> MacroCommands = [];

        public MinimapPointMacros Copy()
        {
            return new MinimapPointMacros
            {
                MacroName = MacroName,
                MacroChance = MacroChance,
                MacroCommands = [.. MacroCommands]
            };
        }
    }


    public class MinimapPointData
    {
        public List<FrameworkElement> ElementTexts = [];

        public string ElementName = "";

        public string PointName = "";

        public List<MinimapPointMacros> Commands = [];

        public MinimapPointData Copy()
        {
            return new MinimapPointData
            {
                ElementTexts = ElementTexts,
                ElementName = ElementName,
                PointName = PointName,
                Commands = Commands.Select(cmd => cmd.Copy()).ToList()
            };
        }
    }


    public class MinimapPoint
    {
        public double X = 0.0;

        public double Y = 0.0;

        public double XRange = 0.0;

        public double YRange = 0.0;

        public MinimapPointData PointData = new MinimapPointData();

        public MinimapPoint Copy()
        {
            return new MinimapPoint
            {
                X = X,
                Y = Y,
                XRange = XRange,
                YRange = YRange,
                PointData = PointData.Copy()
            };
        }

        public void Assign(MinimapPoint point)
        {
            X = point.X;
            Y = point.Y;
            XRange = point.XRange;
            YRange = point.YRange;
            PointData = point.PointData.Copy();
        }
    }


    public abstract class AbstractMapModel
    {
        public abstract List<MinimapPoint> Points();

        public abstract void Add(MinimapPoint point);

        public abstract void Edit(MinimapPoint point);

        public abstract MinimapPoint? FindName(string name);

        public abstract MinimapPoint? FindLabel(string label);

        public abstract void Remove(string name);

        public abstract void Clear();

        public abstract Rect GetMapArea();

        public abstract void SetMapArea(int left, int top, int right, int bottom);

        public abstract float GetTemplateThreshold(string templateKey);

        public abstract void SetTemplateThreshold(string templateKey, float threshold);

        public abstract Tuple<int, int> GetTemplatePosition(string templateKey);

        public abstract void SetTemplatePosition(string templateKey, int x, int y);

        public abstract void SetMapModel(MapModel model);
    }


    public class MapModel : AbstractMapModel
    {
        private ReaderWriterLockSlim _pointsLock = new ReaderWriterLockSlim();

        private List<MinimapPoint> _points = [];

        private ReaderWriterLockSlim _mapAreaLock = new ReaderWriterLockSlim();

        private Rect _mapArea = new Rect(0, 0, 1, 1);

        private ConcurrentDictionary<string, Tuple<int, int>> _templatePosition = [];

        private ConcurrentDictionary<string, float> _templateThreshold = [];

        public override List<MinimapPoint> Points()
        {
            var pointsCopy = new List<MinimapPoint>();
            try
            {
                _pointsLock.EnterReadLock();
                foreach (var point in _points)
                {
                    pointsCopy.Add(point.Copy());
                }
            }
            finally
            {
                _pointsLock.ExitReadLock();
            }
            return pointsCopy;
        }

        public override void Edit(MinimapPoint point)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                var foundPoint = _points.FirstOrDefault(
                    p => p.PointData.ElementName == point.PointData.ElementName
                );
                if (foundPoint != null)
                {
                    foundPoint.Assign(point);
                }
            }
            finally
            {
                _pointsLock.ExitWriteLock();
            }
        }

        public override MinimapPoint? FindName(string name)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                var foundPoint = _points.FirstOrDefault(
                    p => p.PointData.ElementName == name
                );
                if (foundPoint != null)
                {
                    return foundPoint.Copy();
                }
                return null;
            }
            finally
            {
                _pointsLock.ExitWriteLock(); 
            }
        }

        public override MinimapPoint? FindLabel(string label)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                var foundPoint = _points.FirstOrDefault(
                    p => p.PointData.PointName == label
                );
                if (foundPoint != null)
                {
                    return foundPoint.Copy();
                }
                return null;
            }
            finally
            {
                _pointsLock.ExitWriteLock();
            }
        }

        public override void Add(MinimapPoint point)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                if (_points.Contains(point))
                {
                    return;
                }
                _points.Add(point);
            }
            finally
            {
                _pointsLock.ExitWriteLock(); 
            }
        }

        public override void Remove(string name)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                var pointToRemove = _points.Find(
                    p => p.PointData.ElementName == name
                );
                if (pointToRemove != null)
                {
                    _points.Remove(pointToRemove);
                }
            }
            finally
            {
                _pointsLock.ExitWriteLock(); 
            }
        }

        public override void Clear()
        {
            try
            {
                _pointsLock.EnterWriteLock();
                _points.Clear();
            }
            finally
            {
                _pointsLock.ExitWriteLock();
            }
        }

        public override Rect GetMapArea()
        {
            try
            {
                _mapAreaLock.EnterReadLock();
                return new Rect(_mapArea.X, _mapArea.Y, _mapArea.Width, _mapArea.Height);
            }
            finally
            { 
                _mapAreaLock.ExitReadLock();
            }
        }

        public override void SetMapArea(int left, int top, int right, int bottom)
        {
            try
            {
                _mapAreaLock.EnterWriteLock();
                _mapArea = new Rect(left, top, right - left, bottom - top);
            }
            finally
            {
                _mapAreaLock.ExitWriteLock();
            }
        }

        public override Tuple<int, int> GetTemplatePosition(string templateKey)
        {
            if (_templatePosition.TryGetValue(templateKey, out Tuple<int, int>? value))
            {
                return value;
            }
            return new Tuple<int, int>(-1, -1);
        }

        public override void SetTemplatePosition(string templateKey, int x, int y)
        {
            _templatePosition.AddOrUpdate(
                templateKey,
                new Tuple<int, int>(x, y),
                (_, __) => {return new Tuple<int, int>(x, y);}
            );
        }

        private ConcurrentDictionary<string, Tuple<int, int>> _copyTemplatePositions()
        {
            var copy = new ConcurrentDictionary<string, Tuple<int, int>>();
            foreach (var kvp in _templatePosition)
            {
                copy.TryAdd(kvp.Key, kvp.Value);
            }
            return copy;
        }

        public override void SetMapModel(MapModel model)
        {
            var mapArea = model.GetMapArea();
            var modelPoints = model.Points();
            var templatePosition = _copyTemplatePositions();
            try
            {
                _mapAreaLock.EnterWriteLock();
                _pointsLock.EnterWriteLock();
                _mapArea = mapArea;
                _points = modelPoints;
                _templatePosition = templatePosition;
            }
            finally
            {
                _mapAreaLock.ExitWriteLock();
                _pointsLock.ExitWriteLock();
            }
        }

        public override float GetTemplateThreshold(string templateKey)
        {
            if (_templateThreshold.TryGetValue(templateKey, out float value))
            {
                return value;
            }
            return MapIconInfo.DefaultThreshold;
        }

        public override void SetTemplateThreshold(string templateKey, float threshold)
        {
            _templateThreshold.AddOrUpdate(
                templateKey,
                threshold,
                (_, __) => { return threshold; }
            );
        }
    }
}
