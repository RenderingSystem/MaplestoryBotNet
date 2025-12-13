using System;

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

        public abstract MinimapPoint? SelectedPoint();

        public abstract void EditSelected(MinimapPoint point);

        public abstract void Add(MinimapPoint point);

        public abstract void RemovePoint(MinimapPoint point);

        public abstract void SelectPoint(MinimapPoint point);

        public abstract void SelectName(string name);

        public abstract void SelectLabel(string label);

        public abstract void Deselect();

        public abstract void Clear();

        public abstract void Translate(double X, double Y);

        public abstract void Move(double X, double Y);
    }


    public class MapModel : AbstractMapModel
    {
        private ReaderWriterLockSlim _pointsLock = new ReaderWriterLockSlim();

        private List<MinimapPoint> _points = [];

        private MinimapPoint? _selectedPoint = null;

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

        public override MinimapPoint? SelectedPoint()
        {
            MinimapPoint? selectedPoint = null;
            try
            {
                _pointsLock.EnterReadLock();
                if (_selectedPoint != null)
                {
                    selectedPoint = _selectedPoint.Copy();
                }

            }
            finally
            {
                _pointsLock.ExitReadLock();
            }
            return selectedPoint;
        }

        public override void EditSelected(MinimapPoint point)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                if (_selectedPoint != null)
                {
                    _selectedPoint.Assign(point);
                }
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

        public override void RemovePoint(MinimapPoint point)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                var pointToRemove = _points.Find(
                    p => p.PointData.ElementName == point.PointData.ElementName
                );
                if (pointToRemove != null)
                {
                    _points.Remove(pointToRemove);
                    _selectedPoint = _selectedPoint == pointToRemove ? null : _selectedPoint;
                }
            }
            finally
            {
                _pointsLock.ExitWriteLock(); 
            }
        }

        public override void SelectPoint(MinimapPoint point)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                if (_points.Contains(point))
                {
                    _selectedPoint = point;
                }
            }
            finally
            {
                _pointsLock.ExitWriteLock();
            }
        }

        public override void SelectName(string name)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                _selectedPoint = _points.Find(
                    point => point.PointData.ElementName == name
                );
            }
            finally
            {
                _pointsLock.ExitWriteLock();
            }
        }

        public override void SelectLabel(string label)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                _selectedPoint = _points.Find(
                    point => point.PointData.PointName == label
                );
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
                _selectedPoint = null;
            }
            finally
            {
                _pointsLock.ExitWriteLock();
            }
        }

        public override void Translate(double X, double Y)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                if (_selectedPoint != null)
                {
                    _selectedPoint.X += X;
                    _selectedPoint.Y += Y;
                }
            }
            finally
            {
                _pointsLock.ExitWriteLock();
            }
        }

        public override void Move(double X, double Y)
        {
            try
            {
                _pointsLock.EnterWriteLock();
                if (_selectedPoint != null)
                {
                    _selectedPoint.X = X;
                    _selectedPoint.Y = Y;
                }
            }
            finally
            {
                _pointsLock.ExitWriteLock();
            }
        }

        public override void Deselect()
        {
            try
            {
                _pointsLock.EnterWriteLock();
                _selectedPoint = null;
            }
            finally
            {
                _pointsLock.ExitWriteLock(); 
            }
        }
    }
}
