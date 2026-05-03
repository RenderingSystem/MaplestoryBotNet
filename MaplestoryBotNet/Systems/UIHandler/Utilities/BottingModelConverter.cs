using MaplestoryBotNet.Systems.Configuration.SubSystems;


namespace MaplestoryBotNet.Systems.UIHandler.Utilities
{
    public abstract class AbstractJsonDataModelConverter
    {
        public abstract object? ToConfiguration(object dataModel);

        public abstract object? ToDataModel(object configuration);
    }


    public class PointMacrosConverter : AbstractJsonDataModelConverter
    {
        public override object? ToConfiguration(object dataModel)
        {
            if (dataModel is not MinimapPointMacros modelPointMacros)
            {
                return null;
            }
            return new ConfigurationPointMacros
            {
                MacroName = modelPointMacros.MacroName,
                MacroChance = modelPointMacros.MacroChance,
                MacroCommands = [.. modelPointMacros.MacroCommands]
            };
        }

        public override object? ToDataModel(object configuration)
        {
            if (configuration is not ConfigurationPointMacros configurationPointMacros)
            {
                return null;
            }
            return new MinimapPointMacros
            {
                MacroName = configurationPointMacros.MacroName,
                MacroChance = configurationPointMacros.MacroChance,
                MacroCommands = [.. configurationPointMacros.MacroCommands]
            };
        }
    }


    public class PointDataConverter : AbstractJsonDataModelConverter
    {
        private AbstractJsonDataModelConverter _pointMacrosConverter;

        public PointDataConverter(
            AbstractJsonDataModelConverter pointMacrosConverter
        )
        {
            _pointMacrosConverter = pointMacrosConverter;
        }

        public override object? ToConfiguration(
            object dataModel
        )
        {
            if (dataModel is not MinimapPointData modelPointData)
            {
                return null;
            }
            return new ConfigurationPointData
            {
                ElementName = modelPointData.ElementName,
                PointName = modelPointData.PointName,
                Commands = (
                    modelPointData.Commands.Select(
                        command => (
                            (ConfigurationPointMacros)
                            _pointMacrosConverter.ToConfiguration(command)!
                        )
                    ).ToList()
                )
            };
        }

        public override object? ToDataModel(
            object configuration
        )
        {
            if (configuration is not ConfigurationPointData configurationPointData)
            {
                return null;
            }
            return new MinimapPointData
            {
                ElementName = configurationPointData.ElementName,
                PointName = configurationPointData.PointName,
                Commands = (
                    configurationPointData.Commands.Select(
                        command => (
                            (MinimapPointMacros)
                            _pointMacrosConverter.ToDataModel(command)!
                        )
                    )
                    .ToList()
                )
            };
        }
    }


    public class MinimapPointConverter : AbstractJsonDataModelConverter
    {
        private AbstractJsonDataModelConverter _pointDataConverter;

        public MinimapPointConverter(
            AbstractJsonDataModelConverter pointDataConverter
        )
        {
            _pointDataConverter = pointDataConverter;
        }

        public override object? ToConfiguration(object dataModel)
        {
            if (dataModel is not MinimapPoint minimapPoint)
            {
                return null;
            }
            return new ConfigurationMinimapPoint
            {
                X = (int)minimapPoint.X,
                Y = (int)minimapPoint.Y,
                XRange = (int)minimapPoint.XRange,
                YRange = (int)minimapPoint.YRange,
                PointData = (
                    (ConfigurationPointData)
                    _pointDataConverter.ToConfiguration(minimapPoint.PointData)!
                )
            };
        }

        public override object? ToDataModel(object configuration)
        {
            if (configuration is not ConfigurationMinimapPoint configurationMinimapPoint)
            {
                return null;
            }
            return new MinimapPoint
            {
                X = configurationMinimapPoint.X,
                Y = configurationMinimapPoint.Y,
                XRange = configurationMinimapPoint.XRange,
                YRange = configurationMinimapPoint.YRange,
                PointData = (
                    (MinimapPointData)
                    _pointDataConverter.ToDataModel(configurationMinimapPoint.PointData)!
                )
            };
        }
    }


    public class RuneFrameMacrosConverter : AbstractJsonDataModelConverter
    {
        public override object? ToConfiguration(object dataModel)
        {
            if (dataModel is not RuneFrameMacro runeFrameMacro)
            {
                return null;
            }
            return new ConfigurationRuneFrameMacro
            {
                ElementName = runeFrameMacro.ElementLabel,
                MacroName = runeFrameMacro.MacroName,
                X = runeFrameMacro.X,
                Y = runeFrameMacro.Y,
                ScaleX = runeFrameMacro.ScaleX,
                ScaleY = runeFrameMacro.ScaleY,
                NextRuneFrame = (
                    runeFrameMacro.NextRuneFrame != null ?
                    runeFrameMacro.NextRuneFrame.FrameData.ElementLabel :
                    ""
                ),
                Radius = runeFrameMacro.Radius,
                PointCommands = [.. runeFrameMacro.PointCommands]
            };
        }

        public override object? ToDataModel(object configuration)
        {
            if (configuration is not ConfigurationRuneFrameMacro configurationRuneFrameMacro)
            {
                return null;
            }
            return new RuneFrameMacro
            {
                ElementLabel = configurationRuneFrameMacro.ElementName,
                MacroName = configurationRuneFrameMacro.MacroName,
                X = configurationRuneFrameMacro.X,
                Y = configurationRuneFrameMacro.Y,
                ScaleX = configurationRuneFrameMacro.ScaleX,
                ScaleY = configurationRuneFrameMacro.ScaleY,
                NextRuneFrame = null,
                Radius = configurationRuneFrameMacro.Radius,
                PointCommands = [.. configurationRuneFrameMacro.PointCommands]
            };
        }
    }


    public class RuneFrameDirectionsConverter : AbstractJsonDataModelConverter
    {
        public override object? ToConfiguration(object dataModel)
        {
            if (dataModel is not RuneFrameDirection runeFrameDirection)
            {
                return null;
            }
            return new ConfigurationRuneFrameDirection
            {
                DirectionName = runeFrameDirection.DirectionName,
                Direction = (int) runeFrameDirection.Direction,
                Distance = runeFrameDirection.Distance,
                DirectionCommands = [.. runeFrameDirection.DirectionCommands]
            };
        }

        public override object? ToDataModel(object configuration)
        {
            if (configuration is not ConfigurationRuneFrameDirection configurationRuneFrameDirection)
            {
                return null;
            }
            return new RuneFrameDirection
            {
                DirectionName = configurationRuneFrameDirection.DirectionName,
                Direction = (RuneFrameDirectionTypes)configurationRuneFrameDirection.Direction,
                Distance = configurationRuneFrameDirection.Distance,
                DirectionCommands = [.. configurationRuneFrameDirection.DirectionCommands]
            };
        }
    }


    public class RuneFrameDataConverter : AbstractJsonDataModelConverter
    {
        private AbstractJsonDataModelConverter _runeFrameMacrosConverter;

        private AbstractJsonDataModelConverter _runeFrameDirectionsConveter;

        public RuneFrameDataConverter(
            AbstractJsonDataModelConverter runeFrameMacrosConverter,
            AbstractJsonDataModelConverter runeFrameDirectionsConverter
        )
        {
            _runeFrameMacrosConverter = runeFrameMacrosConverter;
            _runeFrameDirectionsConveter = runeFrameDirectionsConverter;
        }

        public override object? ToConfiguration(object dataModel)
        {
            if (dataModel is not RuneFrameData runeFrameData)
            {
                return null;
            }
            return new ConfigurationRuneFrameData
            {
                ElementName = runeFrameData.ElementLabel,
                FrameName = runeFrameData.FrameName,
                RuneFrameMacros = (
                    runeFrameData.RuneFrameMacros.Select(
                        runeFrameMacro => (
                            (ConfigurationRuneFrameMacro)
                            _runeFrameMacrosConverter.ToConfiguration(runeFrameMacro)!
                        )
                    ).ToList()
                ),
                RuneFrameDirections = (
                    runeFrameData.RuneFrameDirections.Select(
                        runeFrameMacro => (
                            (ConfigurationRuneFrameDirection)
                            _runeFrameDirectionsConveter.ToConfiguration(runeFrameMacro)!
                        )
                    ).ToList()
                ),
            };
        }

        public override object? ToDataModel(object configuration)
        {
            if (configuration is not ConfigurationRuneFrameData configurationRuneFrameData)
            {
                return null;
            }
            return new RuneFrameData
            {
                ElementLabel = configurationRuneFrameData.ElementName,
                FrameName = configurationRuneFrameData.FrameName,
                RuneFrameMacros = (
                    configurationRuneFrameData.RuneFrameMacros.Select(
                        configurationRuneFrameMacro => (
                            (RuneFrameMacro)
                            _runeFrameMacrosConverter.ToDataModel(configurationRuneFrameMacro)!
                        )
                    ).ToList()
                ),
                RuneFrameDirections = (
                    configurationRuneFrameData.RuneFrameDirections.Select(
                        configurationRuneFrameDirection => (
                            (RuneFrameDirection)
                            _runeFrameDirectionsConveter.ToDataModel(configurationRuneFrameDirection)!
                        )
                    ).ToList()
                )

            };
        }
    }


    public class RuneFrameConverter : AbstractJsonDataModelConverter
    {
        private AbstractJsonDataModelConverter _runeFrameDataConverter;

        public RuneFrameConverter(
            AbstractJsonDataModelConverter runeFrameDataConverter
        )
        {
            _runeFrameDataConverter = runeFrameDataConverter;
        }

        public override object? ToConfiguration(object dataModel)
        {
            if (dataModel is not RuneFrame runeFrame)
            {
                return null;
            }
            return new ConfigurationRuneFrame
            {
                X = runeFrame.X,
                Y = runeFrame.Y,
                Width = runeFrame.Width,
                Height = runeFrame.Height,
                RuneFrameData = (
                    (ConfigurationRuneFrameData)
                    _runeFrameDataConverter.ToConfiguration(runeFrame.FrameData)!
                )
            };
        }

        public override object? ToDataModel(object configuration)
        {
            if (configuration is not ConfigurationRuneFrame configurationRuneFrame)
            {
                return null;
            }
            return new RuneFrame
            {
                X = configurationRuneFrame.X,
                Y = configurationRuneFrame.Y,
                Width = configurationRuneFrame.Width,
                Height = configurationRuneFrame.Height,
                FrameData = (
                    (RuneFrameData)
                    _runeFrameDataConverter.ToDataModel(configurationRuneFrame.RuneFrameData)!
                )
            };
        }
    }


    public class BottingModelConverter : AbstractJsonDataModelConverter
    {
        private AbstractJsonDataModelConverter _minimapPointConverter;

        private AbstractJsonDataModelConverter _runeFrameConverter;

        public BottingModelConverter(
            AbstractJsonDataModelConverter minimapPointConverter,
            AbstractJsonDataModelConverter runeFrameConverter
        )
        {
            _minimapPointConverter = minimapPointConverter;
            _runeFrameConverter = runeFrameConverter;
        }

        public override object? ToConfiguration(object dataModel)
        {
            if (dataModel is not AbstractBottingModel bottingModel)
            {
                return null;
            }
            var mapModel = bottingModel.GetMapModel();
            var macroModel = bottingModel.GetMacroModel();
            var runeModel = bottingModel.GetRuneModel();
            var mapArea = mapModel.GetMapArea();
            return new ConfigurationBottingModel
            {
                MapAreaLeft = (int)mapArea.Left,
                MapAreaTop = (int)mapArea.Top,
                MapAreaRight = (int)mapArea.Right,
                MapAreaBottom = (int)mapArea.Bottom,
                CharacterThreshold = mapModel.GetTemplateThreshold(MapIconInfo.Character),
                RuneThreshold = mapModel.GetTemplateThreshold(MapIconInfo.Rune),
                MapPoints = macroModel.MacroPoints().Select(
                    minimapPoint => (
                        (ConfigurationMinimapPoint)
                        _minimapPointConverter.ToConfiguration(minimapPoint)!
                    )
                ).ToList(),
                RuneFrames = runeModel.RuneFrames().Select(
                    runeFrame => (
                        (ConfigurationRuneFrame)
                        _runeFrameConverter.ToConfiguration(runeFrame)!
                    )
                ).ToList(),
                RuneRadius = runeModel.GetRadius(),
                RuneActivation = runeModel.GetCooldown(),
                UniformMovement = runeModel.GetUniformMovement()
            };
        }

        private void _setupMapAreaDataModel(
            AbstractBottingModel bottingModel,
            ConfigurationBottingModel configurationBottingModel
        )
        {
            bottingModel.GetMapModel().SetMapArea(
                Math.Min(configurationBottingModel.MapAreaLeft, configurationBottingModel.MapAreaRight),
                Math.Min(configurationBottingModel.MapAreaTop, configurationBottingModel.MapAreaBottom),
                Math.Max(configurationBottingModel.MapAreaLeft, configurationBottingModel.MapAreaRight),
                Math.Max(configurationBottingModel.MapAreaTop, configurationBottingModel.MapAreaBottom)
            );
        }

        private void _setupThresholdDataModel(
            AbstractBottingModel bottingModel,
            ConfigurationBottingModel configurationBottingModel
        )
        {
            bottingModel.GetMapModel().SetTemplateThreshold(
                MapIconInfo.Character, configurationBottingModel.CharacterThreshold
            );
            bottingModel.GetMapModel().SetTemplateThreshold(
                MapIconInfo.Rune, configurationBottingModel.RuneThreshold
            );
        }

        private void _setupMinimapPointDataModel(
            AbstractBottingModel bottingModel,
            ConfigurationBottingModel configurationBottingModel
        )
        {
            foreach (var configurationMinimapPoint in configurationBottingModel.MapPoints)
            {
                var minimapPoint = (MinimapPoint)_minimapPointConverter.ToDataModel(configurationMinimapPoint)!;
                bottingModel.GetMacroModel().AddMacroPoint(minimapPoint);
            }
        }

        private void _setupRuneFrameDataModel(
            AbstractBottingModel bottingModel,
            ConfigurationBottingModel configurationBottingModel
        )
        {
            var runeFrames = new List<RuneFrame>();
            var configurationRuneFrames = configurationBottingModel.RuneFrames;
            for (int i = 0; i < configurationRuneFrames.Count; i++)
            {
                var runeFrame = (RuneFrame)_runeFrameConverter.ToDataModel(configurationRuneFrames[i])!;
                runeFrames.Add(runeFrame);
            }
            for (int i = 0; i < configurationRuneFrames.Count; i++)
            {
                var configurationRuneFrameMacros = configurationRuneFrames[i].RuneFrameData.RuneFrameMacros;
                for (int j = 0; j < configurationRuneFrameMacros.Count; j++)
                {
                    runeFrames[i].FrameData.RuneFrameMacros[j].NextRuneFrame = runeFrames.Find(
                        runeFrame => runeFrame.FrameData.ElementLabel == configurationRuneFrameMacros[j].NextRuneFrame
                    );
                }
            }
            for (int i = 0; i < runeFrames.Count; i++)
            {
                bottingModel.GetRuneModel().AddRuneFrame(runeFrames[i]);
            }
            bottingModel.GetRuneModel().SetRadius(configurationBottingModel.RuneRadius);
            bottingModel.GetRuneModel().SetCooldown(configurationBottingModel.RuneActivation);
            bottingModel.GetRuneModel().SetUniformMovement(configurationBottingModel.UniformMovement);
        }

        public override object? ToDataModel(object configuration)
        {
            if (configuration is not ConfigurationBottingModel configurationBottingModel)
            {
                return null;
            }
            var bottingModel = new BottingModel();
            _setupMapAreaDataModel(bottingModel, configurationBottingModel);
            _setupThresholdDataModel(bottingModel, configurationBottingModel);
            _setupMinimapPointDataModel(bottingModel, configurationBottingModel);
            _setupRuneFrameDataModel(bottingModel, configurationBottingModel);
            return bottingModel;
        }
    }


    public class BottingModelConverterFacade : AbstractJsonDataModelConverter
    {
        private AbstractJsonDataModelConverter _bottingModelConverter;

        public BottingModelConverterFacade()
        {
            _bottingModelConverter = new BottingModelConverter(
                new MinimapPointConverter(
                    new PointDataConverter(
                        new PointMacrosConverter()
                    )
                ),
                new RuneFrameConverter(
                    new RuneFrameDataConverter(
                        new RuneFrameMacrosConverter(),
                        new RuneFrameDirectionsConverter()
                    )
                )
            );
        }

        public override object? ToConfiguration(object dataModel)
        {
            return _bottingModelConverter.ToConfiguration(dataModel);
        }

        public override object? ToDataModel(object configuration)
        {
            return _bottingModelConverter.ToDataModel(configuration);
        }
    }
}
