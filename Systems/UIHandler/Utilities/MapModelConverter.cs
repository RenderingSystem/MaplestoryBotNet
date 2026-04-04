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
                    )
                    .ToList()
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


    public class BottingModelConverter : AbstractJsonDataModelConverter
    {
        private AbstractJsonDataModelConverter _minimapPointConverter;

        public BottingModelConverter(
            AbstractJsonDataModelConverter minimapPointConverter
        )
        {
            _minimapPointConverter = minimapPointConverter;
        }

        public override object? ToConfiguration(object dataModel)
        {
            if (dataModel is not AbstractBottingModel bottingModel)
            {
                return null;
            }
            var mapModel = bottingModel.GetMapModel();
            var macroModel = bottingModel.GetMacroModel();
            var mapArea = mapModel.GetMapArea();
            return new ConfigurationMapModel
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
                )
                .ToList()
            };
        }

        public override object? ToDataModel(object configuration)
        {
            if (configuration is not ConfigurationMapModel configurationMapModel)
            {
                return null;
            }
            var bottingModel = new BottingModel();
            var mapModel = bottingModel.GetMapModel();
            var macroModel = bottingModel.GetMacroModel();
            mapModel.SetMapArea(
                Math.Min(configurationMapModel.MapAreaLeft, configurationMapModel.MapAreaRight),
                Math.Min(configurationMapModel.MapAreaTop, configurationMapModel.MapAreaBottom),
                Math.Max(configurationMapModel.MapAreaLeft, configurationMapModel.MapAreaRight),
                Math.Max(configurationMapModel.MapAreaTop, configurationMapModel.MapAreaBottom)
            );
            mapModel.SetTemplateThreshold(
                MapIconInfo.Character, configurationMapModel.CharacterThreshold
            );
            mapModel.SetTemplateThreshold(
                MapIconInfo.Rune, configurationMapModel.RuneThreshold
            );
            foreach (
                var minimapPoint in configurationMapModel.MapPoints.Select(
                    (configurationMinimapPoint) => {
                        return (MinimapPoint) (
                            _minimapPointConverter.ToDataModel(configurationMinimapPoint)!
                        );
                    }
                )
            )
            {
                macroModel.AddMacroPoint(minimapPoint);
            }
            return bottingModel;
        }
    }


    public class BottingModelConverterFacade : AbstractJsonDataModelConverter
    {
        private AbstractJsonDataModelConverter _mapModelConverter;

        public BottingModelConverterFacade()
        {
            _mapModelConverter = new BottingModelConverter(
                new MinimapPointConverter(
                    new PointDataConverter(
                        new PointMacrosConverter()
                    )
                )
            );
        }

        public override object? ToConfiguration(object dataModel)
        {
            return _mapModelConverter.ToConfiguration(dataModel);
        }

        public override object? ToDataModel(object configuration)
        {
            return _mapModelConverter.ToDataModel(configuration);
        }
    }
}
