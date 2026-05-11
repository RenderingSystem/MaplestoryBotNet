using MaplestoryBotNet.Systems.Configuration.SubSystems;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Navigation;


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
                ElementTexts = [.. ElementTexts],
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


    public abstract class AbstractMacroModel
    {
        public abstract List<MinimapPoint> MacroPoints();

        public abstract void AddMacroPoint(MinimapPoint point);

        public abstract void EditMacroPoint(MinimapPoint point);

        public abstract MinimapPoint? FindMacroPointByName(string name);

        public abstract MinimapPoint? FindMacroPointByLabel(string label);

        public abstract void RemoveMacroPointByName(string name);

        public abstract void ClearMacroPoints();

        public abstract void SetMacroModel(AbstractMacroModel model);

        public abstract AbstractMacroModel Copy();
    }


    public abstract class AbstractMapModel
    {
        public abstract Rect GetMapArea();

        public abstract void SetMapArea(int left, int top, int right, int bottom);

        public abstract float GetTemplateThreshold(string templateKey);

        public abstract void SetTemplateThreshold(string templateKey, float threshold);

        public abstract Tuple<int, int> GetTemplatePosition(string templateKey);

        public abstract void SetTemplatePosition(string templateKey, int x, int y);

        public abstract void SetMapModel(AbstractMapModel model);

        public abstract AbstractMapModel Copy();
    }


    public abstract class AbstractRuneModel
    {
        public abstract List<RuneFrame> RuneFrames();

        public abstract void AddRuneFrame(RuneFrame frame);

        public abstract void EditRuneFrame(RuneFrame frame);

        public abstract RuneFrame? FindRuneFrameByName(string name);

        public abstract RuneFrame? FindRuneFrameRefByLabel(string label);

        public abstract void RemoveRuneFrameByName(string name);

        public abstract void ClearRuneFrames();

        public abstract void SetRuneModel(AbstractRuneModel model);

        public abstract void SetCooldown(int seconds);

        public abstract int GetCooldown();

        public abstract void SetActivation(int seconds);

        public abstract int GetActivation();

        public abstract void SetRadius(int radius);

        public abstract int GetRadius();

        public abstract void SetUniformMovement(int uniformMovement);

        public abstract int GetUniformMovement();

        public abstract List<string> NextNavigation(Point initialPoint, Point finalPoint);

        public abstract AbstractRuneModel Copy();
    }


    public abstract class AbstractBottingModel
    {
        public abstract AbstractMacroModel GetMacroModel();

        public abstract AbstractMapModel GetMapModel();

        public abstract AbstractRuneModel GetRuneModel();

        public abstract AbstractAilmentsModel GetAilmentsModel();

        public abstract void SetBottingModel(AbstractBottingModel model);

        public abstract AbstractBottingModel Copy();
    }


    public abstract class AbstractAilmentsModel
    {
        public abstract void SetAilmentsModel(AbstractAilmentsModel model);

        public abstract List<Tuple<string, int>> GetAilments();

        public abstract int GetAilment(string ailment);

        public abstract void SetAilment(string ailment, int status);

        public abstract AbstractAilmentsModel Copy();
    }


    public class BottingModel : AbstractBottingModel
    {
        private volatile AbstractMacroModel _macroModel;

        private volatile AbstractMapModel _mapModel;

        private volatile AbstractRuneModel _runeModel;

        private volatile AbstractAilmentsModel _ailmentsModel;

        public BottingModel()
        {
            _macroModel = new MacroModel();
            _mapModel = new MapModel();
            _runeModel = new RuneModel();
            _ailmentsModel = new AilmentsModel();
        }

        public override AbstractMacroModel GetMacroModel()
        {
            return _macroModel;
        }

        public override AbstractMapModel GetMapModel()
        {
            return _mapModel;
        }

        public override AbstractRuneModel GetRuneModel()
        {
            return _runeModel;
        }

        public override AbstractAilmentsModel GetAilmentsModel()
        {
            return _ailmentsModel;
        }

        public override void SetBottingModel(AbstractBottingModel model)
        {
            _macroModel.SetMacroModel(model.GetMacroModel().Copy());
            _mapModel.SetMapModel(model.GetMapModel().Copy());
            _runeModel.SetRuneModel(model.GetRuneModel().Copy());
            _ailmentsModel.SetAilmentsModel(model.GetAilmentsModel().Copy());
        }

        public override AbstractBottingModel Copy()
        {
            return new BottingModel
            {
                _macroModel = _macroModel.Copy(),
                _mapModel = _mapModel.Copy(),
                _runeModel = _runeModel.Copy(),
                _ailmentsModel = _ailmentsModel.Copy()
            };
        }

    }


    public class MacroModel : AbstractMacroModel
    {
        private ReaderWriterLockSlim _pointsLock = new ReaderWriterLockSlim();

        private List<MinimapPoint> _points = [];

        public override List<MinimapPoint> MacroPoints()
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

        public override void EditMacroPoint(MinimapPoint point)
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

        public override MinimapPoint? FindMacroPointByName(string name)
        {
            try
            {
                _pointsLock.EnterReadLock();
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
                _pointsLock.ExitReadLock();
            }
        }

        public override MinimapPoint? FindMacroPointByLabel(string label)
        {
            try
            {
                _pointsLock.EnterReadLock();
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
                _pointsLock.ExitReadLock();
            }
        }

        public override void AddMacroPoint(MinimapPoint point)
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

        public override void RemoveMacroPointByName(string name)
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

        public override void ClearMacroPoints()
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

        public override void SetMacroModel(AbstractMacroModel model)
        {
            var modelPoints = model.MacroPoints();
            try
            {
                _pointsLock.EnterWriteLock();
                _points = modelPoints;
            }
            finally
            {
                _pointsLock.ExitWriteLock();
            }
        }

        public override AbstractMacroModel Copy()
        {
            var macroModel = new MacroModel();
            try
            {
                _pointsLock.EnterWriteLock();
                for (int i = 0; i < _points.Count; i++)
                {
                    macroModel._points.Add(_points[i].Copy());
                }
            }
            finally
            {
                _pointsLock.ExitWriteLock();
            }
            return macroModel;
        }
    }


    public class MapModel : AbstractMapModel
    {
        private Rect _mapArea = new Rect(0, 0, 1, 1);

        private ConcurrentDictionary<string, Tuple<int, int>> _templatePosition = [];

        private ConcurrentDictionary<string, float> _templateThresholds = [];

        public override Rect GetMapArea()
        {
            var mapArea = _mapArea;
            return new Rect(mapArea.X, mapArea.Y, mapArea.Width, mapArea.Height);
        }

        public override void SetMapArea(int left, int top, int right, int bottom)
        {
            _mapArea = new Rect(left, top, right - left, bottom - top);
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

        public override float GetTemplateThreshold(string templateKey)
        {
            if (_templateThresholds.TryGetValue(templateKey, out float value))
            {
                return value;
            }
            return MapIconInfo.DefaultThreshold;
        }

        public override void SetTemplateThreshold(string templateKey, float threshold)
        {
            _templateThresholds.AddOrUpdate(
                templateKey,
                threshold,
                (_, __) => { return threshold; }
            );
        }

        public override void SetMapModel(AbstractMapModel model)
        {
            if (model is not MapModel mapModel)
            {
                return;
            }
            _mapArea = mapModel.GetMapArea();
            _templateThresholds.Clear();
            foreach (var kvp in mapModel._templateThresholds)
            {
                _templateThresholds.TryAdd(kvp.Key, kvp.Value);
            }
        }


        public override AbstractMapModel Copy()
        {
            var mapModel = new MapModel() { _mapArea = GetMapArea() };
            foreach (var kvp in _templateThresholds)
            {
                mapModel.SetTemplateThreshold(kvp.Key, kvp.Value);
            }
            return mapModel;
        }
    }


    public class RuneFrameMacro
    {
        public string MacroName = "";

        public string ElementLabel = "";

        public double X = 0.0;

        public double Y = 0.0;

        public double ScaleX = 0.0;

        public double ScaleY = 0.0;

        public RuneFrame? NextRuneFrame = null;

        public double Radius = 0.0;

        public List<string> PointCommands = [];

        public List<FrameworkElement> TextDependencies = [];

        public RuneFrameMacro Copy()
        {
            return new RuneFrameMacro
            {
                MacroName = MacroName,
                ElementLabel = ElementLabel,
                X = X,
                Y = Y,
                ScaleX = ScaleX,
                ScaleY = ScaleY,
                NextRuneFrame = NextRuneFrame,
                Radius = Radius,
                PointCommands = [.. PointCommands],
                TextDependencies = [.. TextDependencies]
            };
        }
    }


    public enum RuneFrameDirectionTypes
    {
        Left = 0,
        Right,
        MaxNum
    }


    public class RuneFrameDirection
    {
        public string DirectionName = "";

        public RuneFrameDirectionTypes Direction = RuneFrameDirectionTypes.Left;

        public int Distance;

        public List<string> DirectionCommands = [];

        public RuneFrameDirection Copy()
        {
            return new RuneFrameDirection
            {
                DirectionName = DirectionName,
                Direction = Direction,
                Distance = Distance,
                DirectionCommands = [.. DirectionCommands]
            };
        }
    }


    public class RuneFrameData
    {
        public List<FrameworkElement> ElementTexts = [];

        public string ElementLabel = "";

        public string FrameName = "";

        public List<RuneFrameMacro> RuneFrameMacros = [];

        public List<RuneFrameDirection> RuneFrameDirections = [];

        public RuneFrameData Copy()
        {
            return new RuneFrameData
            {
                ElementTexts = [.. ElementTexts],
                ElementLabel = ElementLabel,
                FrameName = FrameName,
                RuneFrameMacros = RuneFrameMacros.Select(cmd => cmd.Copy()).ToList(),
                RuneFrameDirections = RuneFrameDirections.Select(cmd => cmd.Copy()).ToList(),
            };
        }
    }

    
    public class RuneFrame
    {
        public double X = 0.0;

        public double Y = 0.0;

        public double Width = 0.0;

        public double Height = 0.0;

        public RuneFrameData FrameData = new RuneFrameData();

        public void Assign(RuneFrame frame)
        {
            X = frame.X;
            Y = frame.Y;
            Width = frame.Width;
            Height = frame.Height;
            FrameData = frame.FrameData.Copy();
        }

        public RuneFrame Copy()
        {
            return new RuneFrame
            {
                X = X,
                Y = Y,
                Width = Width,
                Height = Height,
                FrameData = FrameData.Copy()
            };
        }
    }


    public class RuneModel : AbstractRuneModel
    {
        private ReaderWriterLockSlim _runeFrameLock = new ReaderWriterLockSlim();

        List<RuneFrame> _runeFrames = [];

        private volatile int _runeCooldown = 0;

        private volatile int _runeActivation = 0;

        private volatile int _runeRadius = 0;

        private volatile int _uniformMovement = 0;

        public override void AddRuneFrame(RuneFrame frame)
        {
            try
            {
                _runeFrameLock.EnterWriteLock();
                if (_runeFrames.Contains(frame))
                {
                    return;
                }
                _runeFrames.Add(frame);
            }
            finally
            {
                _runeFrameLock.ExitWriteLock();
            }
        }

        public override void ClearRuneFrames()
        {
            try
            {
                _runeFrameLock.EnterWriteLock();
                _runeFrames.Clear();
            }
            finally
            {
                _runeFrameLock.ExitWriteLock();
            }
        }

        public override void EditRuneFrame(RuneFrame frame)
        {
            try
            {
                _runeFrameLock.EnterWriteLock();
                var frameToEdit = _runeFrames.Find(
                    f => f.FrameData.ElementLabel == frame.FrameData.ElementLabel
                );
                frameToEdit?.Assign(frame);
            }
            finally
            {
                _runeFrameLock.ExitWriteLock();
            }
        }

        public override RuneFrame? FindRuneFrameRefByLabel(string label)
        {
            try
            {
                _runeFrameLock.EnterReadLock();
                var foundPoint = _runeFrames.FirstOrDefault(
                    p => p.FrameData.FrameName == label
                );
                return foundPoint;
            }
            finally
            {
                _runeFrameLock.ExitReadLock();
            }
        }

        public override RuneFrame? FindRuneFrameByName(string name)
        {
            try
            {
                _runeFrameLock.EnterReadLock();
                var foundFrame = _runeFrames.FirstOrDefault(
                    p => p.FrameData.ElementLabel == name
                );
                return foundFrame?.Copy();
            }
            finally
            {
                _runeFrameLock.ExitReadLock();
            }
        }

        public override void RemoveRuneFrameByName(string name)
        {
            try
            {
                _runeFrameLock.EnterWriteLock();
                var foundFrame = _runeFrames.FirstOrDefault(
                    p => p.FrameData.ElementLabel == name
                );
                if (foundFrame != null)
                {
                    _runeFrames.Remove(foundFrame);
                    for (int i = 0; i < _runeFrames.Count; i++)
                    {
                        for (int j = 0; j < _runeFrames[i].FrameData.RuneFrameMacros.Count; j++)
                        {
                            var runeFrameMacro = _runeFrames[i].FrameData.RuneFrameMacros[j];
                            if (runeFrameMacro.NextRuneFrame == foundFrame)
                            {
                                runeFrameMacro.NextRuneFrame = null;
                            }
                        }
                    }
                }
            }
            finally
            {
                _runeFrameLock.ExitWriteLock();
            }
        }


        private List<RuneFrame> _getRuneFrames()
        {
            var runeFrames = _runeFrames.Select(rf => rf.Copy()).ToList();
            for (int i = 0; i < runeFrames.Count; i++)
            {
                var runeFrameMacros = runeFrames[i].FrameData.RuneFrameMacros;
                for (int j = 0; j < runeFrameMacros.Count; j++)
                {
                    if (runeFrameMacros[j].NextRuneFrame != null)
                    {
                        var nextRuneFrame = runeFrameMacros[j].NextRuneFrame;
                        var nextElementLabel = nextRuneFrame!.FrameData.ElementLabel;
                        runeFrameMacros[j].NextRuneFrame = runeFrames.Find(
                            rf => rf.FrameData.ElementLabel == nextElementLabel
                        );
                    }
                }
            }
            return runeFrames;
        }

        public override List<RuneFrame> RuneFrames()
        {
            try
            {
                _runeFrameLock.EnterReadLock();
                return _getRuneFrames();
            }
            finally
            {
                _runeFrameLock.ExitReadLock();
            }
        }

        public override void SetRuneModel(AbstractRuneModel model)
        {
            try
            {
                _runeFrameLock.EnterWriteLock();
                var newRuneModel = model.Copy();
                _runeFrames.Clear();
                _runeFrames.AddRange(newRuneModel.RuneFrames());
                _runeCooldown = newRuneModel.GetCooldown();
                _runeActivation = newRuneModel.GetActivation();
                _runeRadius = newRuneModel.GetRadius();
                _uniformMovement = newRuneModel.GetUniformMovement();
            }
            finally
            {
                _runeFrameLock.ExitWriteLock();
            }
        }

        public override AbstractRuneModel Copy()
        {
            try
            {
                _runeFrameLock.EnterReadLock();
                return new RuneModel
                {
                    _runeCooldown = _runeCooldown,
                    _runeActivation = _runeActivation,
                    _runeRadius = _runeRadius,
                    _uniformMovement = _uniformMovement,
                    _runeFrames = _getRuneFrames()
                };
            }
            finally
            {
                _runeFrameLock.ExitReadLock();
            }
        }

        public override void SetCooldown(int seconds)
        {
            try
            {
                _runeFrameLock.EnterWriteLock();
                _runeCooldown = seconds;
            }
            finally
            {
                _runeFrameLock.ExitWriteLock();
            }
        }

        public override int GetCooldown()
        {
            try
            {
                _runeFrameLock.EnterWriteLock();
                return _runeCooldown;
            }
            finally
            {
                _runeFrameLock.ExitWriteLock();
            }
        }

        public override void SetActivation(int seconds)
        {
            try
            {
                _runeFrameLock.EnterWriteLock();
                _runeActivation = seconds;
            }
            finally
            {
                _runeFrameLock.ExitWriteLock();
            }
        }

        public override int GetActivation()
        {
            try
            {
                _runeFrameLock.EnterReadLock();
                return _runeActivation;
            }
            finally
            {
                _runeFrameLock.ExitReadLock();
            }
        }

        public override void SetRadius(int radius)
        {
            try
            {
                _runeFrameLock.EnterWriteLock();
                _runeRadius = radius;
            }
            finally
            {
                _runeFrameLock.ExitWriteLock();
            }
        }

        public override int GetRadius()
        {
            try
            {
                _runeFrameLock.EnterReadLock();
                return _runeRadius;
            }
            finally
            {
                _runeFrameLock.ExitReadLock();
            }
        }

        public override void SetUniformMovement(int uniformMovement)
        {
            try
            {
                _runeFrameLock.EnterWriteLock();
                _uniformMovement = uniformMovement;
            }
            finally
            {
                _runeFrameLock.ExitWriteLock();
            }
        }

        public override int GetUniformMovement()
        {
            try
            {
                _runeFrameLock.EnterReadLock();
                return _uniformMovement;
            }
            finally
            {
                _runeFrameLock.ExitReadLock();
            }
        }

        private List<RuneFrame>? _shortestPath(RuneFrame start, RuneFrame destination)
        {
            var queue = new Queue<List<RuneFrame>>();
            var visited = new HashSet<string> { start.FrameData.ElementLabel };
            queue.Enqueue(new List<RuneFrame> { start });
            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var current = path.Last();
                foreach (var macro in current.FrameData.RuneFrameMacros)
                {
                    if (macro.NextRuneFrame == null)
                    {
                        continue;
                    }
                    var next = macro.NextRuneFrame;
                    var newPath = new List<RuneFrame>(path) { next };
                    if (next.FrameData.ElementLabel == destination.FrameData.ElementLabel)
                    {
                        return newPath;
                    }
                    if (!visited.Contains(next.FrameData.ElementLabel))
                    {
                        visited.Add(next.FrameData.ElementLabel);
                        queue.Enqueue(newPath);
                    }
                }
            }
            return null;
        }

        private RuneFrameMacro? _selectNextMacro(
            RuneFrame currentFrame, RuneFrame destinationFrame
        )
        {
            if (currentFrame == destinationFrame)
            {
                return null;
            }
            var framePath = _shortestPath(currentFrame, destinationFrame);
            if (framePath == null || framePath.Count < 2)
            {
                return null;
            }
            var currFrameMacro = currentFrame.FrameData.RuneFrameMacros
                .FirstOrDefault(m => m.NextRuneFrame == framePath[1]);
            return currFrameMacro;
        }

        private RuneFrame? _selectClosestRuneFrame(Point position)
        {
            var frames = _runeFrames;
            if (frames == null || frames.Count == 0)
            {
                return null;
            }
            RuneFrame? closestFrame = null;
            var closestDistance = double.MaxValue;
            foreach (var frame in frames)
            {
                var tl = new Point(frame.X, frame.Y);
                var tr = new Point(frame.X + frame.Width, frame.Y);
                var bl = new Point(frame.X, frame.Y + frame.Height);
                var br = new Point(frame.X + frame.Width, frame.Y + frame.Height);
                var currentDistance = double.MaxValue;
                if ((position.X >= tl.X && position.X <= tr.X) && (position.Y >= tl.Y && position.Y <= bl.Y))
                {
                    return frame;
                }
                else if (position.X >= tl.X && position.X <= tr.X)
                {
                    currentDistance = (position.Y < tl.Y) ? tl.Y - position.Y : position.Y - bl.Y;
                }
                else if (position.Y >= tl.Y && position.Y <= bl.Y)
                {
                    currentDistance = (position.X < tl.X) ? tl.X - position.X : position.X - tr.X;
                }
                else
                {
                    var distToTL = Math.Sqrt(Math.Pow(position.X - tl.X, 2) + Math.Pow(position.Y - tl.Y, 2));
                    var distToTR = Math.Sqrt(Math.Pow(position.X - tr.X, 2) + Math.Pow(position.Y - tr.Y, 2));
                    var distToBL = Math.Sqrt(Math.Pow(position.X - bl.X, 2) + Math.Pow(position.Y - bl.Y, 2));
                    var distToBR = Math.Sqrt(Math.Pow(position.X - br.X, 2) + Math.Pow(position.Y - br.Y, 2));
                    currentDistance = Math.Min(Math.Min(distToTL, distToTR), Math.Min(distToBL, distToBR));
                }
                if (currentDistance < closestDistance)
                {
                    closestDistance = currentDistance;
                    closestFrame = frame;
                }
            }
            return closestFrame;
        }

        private RuneFrameDirection? _nextDirection(
            RuneFrame runeFrame,
            double proximity,
            Point initialPoint,
            Point finalPoint
        )
        {
            var dx = finalPoint.X - initialPoint.X;
            var directions = runeFrame.FrameData.RuneFrameDirections;
            if (Math.Abs(dx) <= proximity || directions.Count == 0)
            {
                return null;
            }
            if (directions.Count == 1)
            {
                return directions[0];
            }
            RuneFrameDirection? bestMovement = null;
            double bestDistance = double.MaxValue;
            for (int i = 0; i < directions.Count; i++)
            {
                var movement = directions[i];
                int directionSign = movement.Direction == RuneFrameDirectionTypes.Left ? -1 : 1;
                double newX = initialPoint.X + (movement.Distance * directionSign);
                double distanceToPoint = Math.Abs(finalPoint.X - newX);
                if (distanceToPoint < bestDistance)
                {
                    bestDistance = distanceToPoint;
                    bestMovement = movement;
                }
            }
            return bestMovement;
        }

        public override List<string> NextNavigation(
            Point initialPoint,
            Point finalPoint
        )
        {
            try
            {
                _runeFrameLock.EnterReadLock();
                var initialRuneFrame = _selectClosestRuneFrame(initialPoint);
                var finalRuneFrame = _selectClosestRuneFrame(finalPoint);
                if (initialRuneFrame != null && finalRuneFrame != null)
                {
                    var selected = _selectNextMacro(initialRuneFrame, finalRuneFrame);

                    if (selected != null)
                    {
                        var localX = initialRuneFrame.Width * (selected.X / selected.ScaleX);
                        var localY = initialRuneFrame.Height * (selected.Y / selected.ScaleY);
                        var macroX = initialRuneFrame.X + localX;
                        var macroY = initialRuneFrame.Y + localY;
                        var macroPoint = new Point(macroX, macroY);
                        var nextDirection = _nextDirection(
                            initialRuneFrame, selected.Radius, initialPoint, macroPoint
                        );
                        return (
                            nextDirection != null ?
                            nextDirection.DirectionCommands : selected.PointCommands
                        );
                    }
                    else
                    {
                        var nextDirection = _nextDirection(
                            initialRuneFrame, _runeRadius, initialPoint, finalPoint
                        );
                        return (
                            nextDirection != null ?
                            nextDirection.DirectionCommands : []
                        );
                    }
                }
                return [];
            }
            finally
            {
                _runeFrameLock.ExitReadLock();
            }
        }
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
