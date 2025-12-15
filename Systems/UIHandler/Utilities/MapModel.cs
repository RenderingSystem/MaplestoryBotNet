using System;
using System.Xml.Linq;

namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public class MinimapPointData
    {
        public string ElementName = "";

        public string PointName = "";

        public List<string> Commands = [];
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
                PointData = new MinimapPointData
                {
                    ElementName = PointData.ElementName,
                    PointName = PointData.PointName,
                    Commands = [.. PointData.Commands],
                }
            };
        }

        public void Assign(MinimapPoint point)
        {
            X = point.X;
            Y = point.Y;
            XRange = point.XRange;
            YRange = point.YRange;
            PointData = new MinimapPointData
            {
                ElementName = point.PointData.ElementName,
                PointName = point.PointData.PointName,
                Commands = [.. point.PointData.Commands],
            };
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

    }


    public class MapModel : AbstractMapModel
    {
        private ReaderWriterLockSlim _pointsLock = new ReaderWriterLockSlim();

        private List<MinimapPoint> _points = [];

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
    }
}
