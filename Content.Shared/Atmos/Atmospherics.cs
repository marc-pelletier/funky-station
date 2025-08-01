// SPDX-FileCopyrightText: 2020 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Metal Gear Sloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <zddm@outlook.es>
// SPDX-FileCopyrightText: 2020 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 py01 <60152240+collinlunn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Errant <35878406+dmnct@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <92357316+Piras314@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 router <messagebus@vk.com>
// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Mish <bluscout78@yahoo.com>
// SPDX-FileCopyrightText: 2025 ReconPangolin <67752926+ReconPangolin@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 rottenheadphones <juaelwe@outlook.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;
// ReSharper disable InconsistentNaming

namespace Content.Shared.Atmos
{
    /// <summary>
    ///     Class to store atmos constants.
    /// </summary>
    public static class Atmospherics
    {
        #region ATMOS
        /// <summary>
        ///     The universal gas constant, in kPa*L/(K*mol)
        /// </summary>
        public const float R = 8.314462618f;

        /// <summary>
        ///     1 ATM in kPA.
        /// </summary>
        public const float OneAtmosphere = 101.325f;

        /// <summary>
        ///     Maximum external pressure (in kPA) a gas miner will, by default, output to.
        ///     This is used to initialize roundstart atmos rooms.
        /// </summary>
        public const float GasMinerDefaultMaxExternalPressure = 6500f;

        /// <summary>
        ///     -270.3ºC in K. CMB stands for Cosmic Microwave Background.
        /// </summary>
        public const float TCMB = 2.7f;

        /// <summary>
        ///     0ºC in K
        /// </summary>
        public const float T0C = 273.15f;

        /// <summary>
        ///     20ºC in K
        /// </summary>
        public const float T20C = 293.15f;

        /// <summary>
        ///     -38.15ºC in K.
        ///     This is used to initialize roundstart freezer rooms.
        /// </summary>
        public const float FreezerTemp = 235f;

        /// <summary>
        ///     Do not allow any gas mixture temperatures to exceed this number. It is occasionally possible
        ///     to have very small heat capacity (e.g. room that was just unspaced) and for large amounts of
        ///     energy to be transferred to it, even for a brief moment. However, this messes up subsequent
        ///     calculations and so cap it here. The physical interpretation is that at this temperature, any
        ///     gas that you would have transforms into plasma.
        /// </summary>
        public const float Tmax = 262144000; // 1/64 of max safe integer, any values above will result in a ~0.03K epsilon // Funky - raised to support HFR

        /// <summary>
        ///     Liters in a cell.
        /// </summary>
        public const float CellVolume = 2500f;

        // Liters in a normal breath
        public const float BreathVolume = 0.5f;

        // Amount of air to take from a tile
        public const float BreathPercentage = BreathVolume / CellVolume;

        /// <summary>
        ///     Moles in a 2.5 m^3 cell at 101.325 kPa and 20ºC
        /// </summary>
        public const float MolesCellStandard = (OneAtmosphere * CellVolume / (T20C * R));

        /// <summary>
        ///     Moles in a 2.5 m^3 cell at 101.325 kPa and -38.15ºC.
        ///     This is used in fix atmos freezer markers to ensure the air is at the correct atmospheric pressure while still being cold.
        /// </summary>
        public const float MolesCellFreezer = (OneAtmosphere * CellVolume / (FreezerTemp * R));

        /// <summary>
        ///     Moles in a 2.5 m^3 cell at GasMinerDefaultMaxExternalPressure kPa and 20ºC
        /// </summary>
        public const float MolesCellGasMiner = (GasMinerDefaultMaxExternalPressure * CellVolume / (T20C * R));

        /// <summary>
        ///     Compared against for superconduction.
        /// </summary>
        public const float MCellWithRatio = (MolesCellStandard * 0.005f);

        public const float OxygenStandard = 0.21f;
        public const float NitrogenStandard = 0.79f;

        public const float OxygenMolesStandard = MolesCellStandard * OxygenStandard;
        public const float NitrogenMolesStandard = MolesCellStandard * NitrogenStandard;

        public const float OxygenMolesFreezer = MolesCellFreezer * OxygenStandard;
        public const float NitrogenMolesFreezer = MolesCellFreezer * NitrogenStandard;

        #endregion

        /// <summary>
        ///     Visible moles multiplied by this factor to get moles at which gas is at max visibility.
        /// </summary>
        public const float FactorGasVisibleMax = 20f;

        /// <summary>
        ///     Minimum number of moles a gas can have.
        /// </summary>
        public const float GasMinMoles = 0.00000005f;

        public const float OpenHeatTransferCoefficient = 0.4f;

        /// <summary>
        ///     Hack to make vacuums cold, sacrificing realism for gameplay.
        /// </summary>
        public const float HeatCapacityVacuum = 7000f;

        /// <summary>
        ///     Ratio of air that must move to/from a tile to reset group processing
        /// </summary>
        public const float MinimumAirRatioToSuspend = 0.1f;

        /// <summary>
        ///     Minimum ratio of air that must move to/from a tile
        /// </summary>
        public const float MinimumAirRatioToMove = 0.001f;

        /// <summary>
        ///     Minimum amount of air that has to move before a group processing can be suspended
        /// </summary>
        public const float MinimumAirToSuspend = (MolesCellStandard * MinimumAirRatioToSuspend);

        public const float MinimumTemperatureToMove = (T20C + 100f);

        public const float MinimumMolesDeltaToMove = (MolesCellStandard * MinimumAirRatioToMove);

        /// <summary>
        ///     Minimum temperature difference before group processing is suspended
        /// </summary>
        public const float MinimumTemperatureDeltaToSuspend = 4.0f;

        /// <summary>
        ///     Minimum temperature difference before the gas temperatures are just set to be equal.
        /// </summary>
        public const float MinimumTemperatureDeltaToConsider = 0.01f;

        /// <summary>
        ///     Minimum temperature for starting superconduction.
        /// </summary>
        public const float MinimumTemperatureStartSuperConduction = (T20C + 400f);
        public const float MinimumTemperatureForSuperconduction = (T20C + 80f);

        /// <summary>
        ///     Minimum heat capacity.
        /// </summary>
        public const float MinimumHeatCapacity = 0.0003f;

        /// <summary>
        ///     For the purposes of making space "colder"
        /// </summary>
        public const float SpaceHeatCapacity = 7000f;

        /// <summary>
        ///     Dictionary of chemical abbreviations for <see cref="Gas"/>
        /// </summary>
        public static Dictionary<Gas, string> GasAbbreviations = new Dictionary<Gas, string>()
        {
            [Gas.Ammonia] = Loc.GetString("gas-ammonia-abbreviation"),
            [Gas.CarbonDioxide] = Loc.GetString("gas-carbon-dioxide-abbreviation"),
            [Gas.Frezon] = Loc.GetString("gas-frezon-abbreviation"),
            [Gas.Nitrogen] = Loc.GetString("gas-nitrogen-abbreviation"),
            [Gas.NitrousOxide] = Loc.GetString("gas-nitrous-oxide-abbreviation"),
            [Gas.Oxygen] = Loc.GetString("gas-oxygen-abbreviation"),
            [Gas.Plasma] = Loc.GetString("gas-plasma-abbreviation"),
            [Gas.Tritium] = Loc.GetString("gas-tritium-abbreviation"),
            [Gas.WaterVapor] = Loc.GetString("gas-water-vapor-abbreviation"),
            [Gas.BZ] = Loc.GetString("gas-bz-abbreviation"),
            [Gas.Healium] = Loc.GetString("gas-healium-abbreviation"),
            [Gas.Nitrium] = Loc.GetString("gas-nitrium-abbreviation"),
            [Gas.Pluoxium] = Loc.GetString("gas-pluoxium-abbreviation"),
            [Gas.Hydrogen] = Loc.GetString("gas-hydrogen-abbreviation"),
            [Gas.HyperNoblium] = Loc.GetString("gas-hyper-noblium-abbreviation"),
            [Gas.ProtoNitrate] = Loc.GetString("gas-proto-nitrate-abbreviation"),
            [Gas.Zauker] = Loc.GetString("gas-zauker-abbreviation"),
            [Gas.Halon] = Loc.GetString("gas-halon-abbreviation"),
            [Gas.Helium] = Loc.GetString("gas-helium-abbreviation"),
            [Gas.AntiNoblium] = Loc.GetString("gas-anti-noblium-abbreviation"),
        };


        /// <summary>
        ///     Funkystation - Dictionary of names for <see cref="Gas"/>
        /// </summary>
        public static Dictionary<Gas, string> GasNames = new Dictionary<Gas, string>()
        {
            [Gas.Ammonia] = Loc.GetString("gases-ammonia"),
            [Gas.CarbonDioxide] = Loc.GetString("gases-co2"),
            [Gas.Frezon] = Loc.GetString("gases-frezon"),
            [Gas.Nitrogen] = Loc.GetString("gases-nitrogen"),
            [Gas.NitrousOxide] = Loc.GetString("gases-n2o"),
            [Gas.Oxygen] = Loc.GetString("gases-oxygen"),
            [Gas.Plasma] = Loc.GetString("gases-plasma"),
            [Gas.Tritium] = Loc.GetString("gases-tritium"),
            [Gas.WaterVapor] = Loc.GetString("gases-water-vapor"),
            [Gas.BZ] = Loc.GetString("gases-bz"),
            [Gas.Healium] = Loc.GetString("gases-healium"),
            [Gas.Nitrium] = Loc.GetString("gases-nitrium"),
            [Gas.Pluoxium] = Loc.GetString("gases-pluoxium"),
            [Gas.Hydrogen] = Loc.GetString("gases-hydrogen"),
            [Gas.HyperNoblium] = Loc.GetString("gases-hyper-noblium"),
            [Gas.ProtoNitrate] = Loc.GetString("gases-proto-nitrate"),
            [Gas.Zauker] = Loc.GetString("gases-zauker"),
            [Gas.Halon] = Loc.GetString("gases-halon"),
            [Gas.Helium] = Loc.GetString("gases-helium"),
            [Gas.AntiNoblium] = Loc.GetString("gases-anti-noblium"),
        };

        #region Excited Groups

        /// <summary>
        ///     Number of full atmos updates ticks before an excited group breaks down (averages gas contents across turfs)
        /// </summary>
        public const int ExcitedGroupBreakdownCycles = 4;

        /// <summary>
        ///     Number of full atmos updates before an excited group dismantles and removes its turfs from active
        /// </summary>
        public const int ExcitedGroupsDismantleCycles = 16;

        #endregion

        /// <summary>
        ///     Hard limit for zone-based tile equalization.
        /// </summary>
        public const int MonstermosHardTileLimit = 2000;

        /// <summary>
        ///     Limit for zone-based tile equalization.
        /// </summary>
        public const int MonstermosTileLimit = 200;

        /// <summary>
        ///     Total number of gases. Increase this if you want to add more!
        /// </summary>
        public const int TotalNumberOfGases = 20; // Assmos - /tg/ gases

        /// <summary>
        ///     This is the actual length of the gases arrays in mixtures.
        ///     Set to the closest multiple of 4 relative to <see cref="TotalNumberOfGases"/> for SIMD reasons.
        /// </summary>
        public const int AdjustedNumberOfGases = ((TotalNumberOfGases + 3) / 4) * 4;

        /// <summary>
        ///     Amount of heat released per mole of burnt hydrogen or tritium (hydrogen isotope)
        /// </summary>
        public const float FireHydrogenEnergyReleased = 284e3f; // hydrogen is 284 kJ/mol
        public const float FireMinimumTemperatureToExist = T0C + 100f;
        public const float FireMinimumTemperatureToSpread = T0C + 150f;
        public const float FireSpreadRadiosityScale = 0.85f;
        public const float FirePlasmaEnergyReleased = 160e3f; // methane is 16 kJ/mol, plus plasma's spark of magic
        public const float FireGrowthRate = 40000f;

        public const float SuperSaturationThreshold = 96f;
        public const float SuperSaturationEnds = SuperSaturationThreshold / 3;

        public const float OxygenBurnRateBase = 1.4f;
        public const float PlasmaMinimumBurnTemperature = (100f+T0C);
        public const float PlasmaUpperTemperature = (1370f+T0C);
        public const float PlasmaOxygenFullburn = 10f;
        public const float PlasmaBurnRateDelta = 9f;
        public const float HydrogenBurnRateDelta = 2f; // Assmos - /tg/ gases

        /// <summary>
        ///     This is calculated to help prevent singlecap bombs (Overpowered tritium/oxygen single tank bombs)
        /// </summary>
        public const float MinimumTritiumOxyburnEnergy = 143000f;

        public const float TritiumBurnOxyFactor = 100f;
        public const float TritiumBurnTritFactor = 10f;

        public const float FrezonCoolLowerTemperature = 23.15f;

        /// <summary>
        ///     Frezon cools better at higher temperatures.
        /// </summary>
        public const float FrezonCoolMidTemperature = 373.15f;

        public const float FrezonCoolMaximumEnergyModifier = 10f;

        /// <summary>
        ///     Remove X mol of nitrogen for each mol of frezon.
        /// </summary>
        public const float FrezonNitrogenCoolRatio = 5;
        public const float FrezonCoolEnergyReleased = -600e3f;
        public const float FrezonCoolRateModifier = 20f;

        public const float FrezonProductionMaxEfficiencyTemperature = 73.15f;

        /// <summary>
        ///     1 mol of N2 is required per X mol of tritium and oxygen.
        /// </summary>
        public const float FrezonProductionNitrogenRatio = 10f;

        /// <summary>
        ///     1 mol of Tritium is required per X mol of oxygen.
        /// </summary>
        public const float FrezonProductionTritRatio = 50.0f;

        /// <summary>
        ///     1 / X of the tritium is converted into Frezon each tick
        /// </summary>
        public const float FrezonProductionConversionRate = 50f;

        /// <summary>
        ///     The maximum portion of the N2O that can decompose each reaction tick. (50%)
        /// </summary>
        public const float N2ODecompositionRate = 2f;

        /// <summary>
        ///     Divisor for Ammonia Oxygen reaction so that it doesn't happen instantaneously.
        /// </summary>
        public const float AmmoniaOxygenReactionRate = 10f;

        /// <summary>
        ///     The amount of energy 1 mole of BZ forming from N2O and plasma releases.
        /// </summary>
        public const float BZProductionEnergy = 80e3f; // Assmos - /tg/ gases

        /// <summary>
        ///     The amount of energy 1 mol of Healium forming from BZ and frezon releases.
        /// </summary>
        public const float HealiumProductionEnergy = 9e3f; // Assmos - /tg/ gases

        /// <summary>
        ///     The amount of energy 1 mol of Nitrium forming from Tritium, Nitrogen and BZ releases.
        /// </summary>
        public const float NitriumProductionEnergy = -100e3f; // Assmos - /tg/ gases

        /// <summary>
        ///     The amount of energy 1 mol of Nitrium decomposing into nitrogen and water vapor releases.
        /// </summary>
        public const float NitriumDecompositionEnergy = 30e3f; // Assmos - /tg/ gases

        /// <summary>
        ///     The amount of energy 1 mol of Pluoxium forming releases.
        /// </summary>
        public const float PluoxiumProductionEnergy = 250; // Assmos - /tg/ gases

        /// <summary>
        ///     The amount of energy 1 mol of Pluoxium forming releases.
        /// </summary>
        public const float MinimumHydrogenOxyburnEnergy = 143000f; // Assmos - /tg/ gases

        public const float HydrogenBurnOxyFactor = 100f; // Assmos - /tg/ gases
        public const float HydrogenBurnH2Factor = 10f; // Assmos - /tg/ gases

        /// <summary>
        ///     The amount of energy 1 mol of hyper-noblium forming from tritium and nitrogen releases.
        /// </summary>
        public const float HyperNobliumProductionEnergy = 2e7f;

        /// <summary>
        /// Energy released per mol of BZ consumed during halon formation.
        /// </summary>
        public const float HalonProductionEnergy = 91232.1f;

        /// <summary>
        /// How much energy a mole of halon combusting consumes.
        /// </summary>
        public const float HalonCombustionEnergy = -2500f;

        /// <summary>
        /// The amount of energy half a mole of zauker forming from hypernoblium and nitrium consumes.
        /// </summary>
        public const float ZaukerProductionEnergy = 5000f;

        /// <summary>
        /// The temperature scaling factor for zauker formation. At most this many moles of zauker can form per reaction tick per kelvin.
        /// </summary>
        public const float ZaukerTemperatureScale = 5e-6f;

        /// <summary>
        /// The amount of energy a mole of zauker decomposing in the presence of nitrogen releases.
        /// </summary>
        public const float ZaukerDecompositionEnergy = 460f;

        /// <summary>
        /// The maximum number of moles of zauker that can decompose per reaction tick.
        /// </summary>

        public const float ZaukerDecompositionMaxRate = 20f;

        /// <summary>
        /// The amount of energy 2.2 moles of proto-nitrate forming from pluoxium and hydrogen releases.
        /// </summary>
        public const float ProtoNitrateProductionEnergy = 650f;

        /// <summary>
        /// The temperature scaling factor for proto-nitrate formation. At most this many moles of zauker can form per reaction tick per kelvin.
        /// </summary>
        public const float ProtoNitrateTemperatureScale = 5e-3f;

        /// <summary>
        /// The maximum number of moles of hydrogen that can be converted into proto-nitrate in a single reaction tick.
        /// </summary>
        public const float ProtoNitrateHydrogenConversionMaxRate = 5f;

        /// <summary>
        /// The amount of energy converting a mole of hydrogen into half a mole of proto-nitrate consumes.
        /// </summary>
        public const float ProtoNitrateHydrogenConversionEnergy = -2500f;

        /// <summary>
        /// The amount of energy proto-nitrate converting a mole of tritium into hydrogen releases.
        /// </summary>
        public const float ProtoNitrateTritiumConversionEnergy = 10000f;

        /// <summary>
        /// The amount of energy proto-nitrate breaking down a mole of BZ releases.
        /// </summary>
        public const float ProtoNitrateBZConversionEnergy = -10000f;



        /// <summary>
        ///     Determines at what pressure the ultra-high pressure red icon is displayed.
        /// </summary>
        public const float HazardHighPressure = 550f;

        /// <summary>
        ///     Determines when the orange pressure icon is displayed.
        /// </summary>
        public const float WarningHighPressure = 0.7f * HazardHighPressure;

        /// <summary>
        ///     Determines when the gray low pressure icon is displayed.
        /// </summary>
        public const float WarningLowPressure = 2.5f * HazardLowPressure;

        /// <summary>
        ///     Determines when the black ultra-low pressure icon is displayed.
        /// </summary>
        public const float HazardLowPressure = 20f;

        /// <summary>
        ///    The amount of pressure damage someone takes is equal to ((pressure / HAZARD_HIGH_PRESSURE) - 1)*PRESSURE_DAMAGE_COEFFICIENT,
        ///     with the maximum of MaxHighPressureDamage.
        /// </summary>
        public const float PressureDamageCoefficient = 4;

        /// <summary>
        ///     Maximum amount of damage that can be endured with high pressure.
        /// </summary>
        public const int MaxHighPressureDamage = 4;

        /// <summary>
        ///     The amount of damage someone takes when in a low pressure area
        ///     (The pressure threshold is so low that it doesn't make sense to do any calculations,
        ///     so it just applies this flat value).
        /// </summary>
        public const int LowPressureDamage = 4;

        public const float WindowHeatTransferCoefficient = 0.1f;

        /// <summary>
        ///     Directions that atmos currently supports. Modify in case of multi-z.
        ///     See <see cref="AtmosDirection"/> on the server.
        /// </summary>
        public const int Directions = 4;

        /// <summary>
        ///     The normal body temperature in degrees Celsius.
        /// </summary>
        public const float NormalBodyTemperature = 37f;

        /// <summary>
        ///     I hereby decree. This is Arbitrary Suck my Dick
        /// </summary>
        public const float BreathMolesToReagentMultiplier = 1144;

        #region Pipes

        /// <summary>
        ///     The default pressure at which pumps and powered equipment max out at, in kPa.
        /// </summary>
        public const float MaxOutputPressure = 4500;

        /// <summary>
        ///     The default maximum speed powered equipment can work at, in L/s.
        /// </summary>
        public const float MaxTransferRate = 200;

        #endregion
    }

    /// <summary>
    ///     Gases to Ids. Keep these updated with the prototypes!
    /// </summary>
    [Serializable, NetSerializable]
    public enum Gas : sbyte
    {
        Oxygen = 0,
        Nitrogen = 1,
        CarbonDioxide = 2,
        Plasma = 3,
        Tritium = 4,
        WaterVapor = 5,
        Ammonia = 6,
        NitrousOxide = 7,
        Frezon = 8,
        BZ = 9, // Assmos - /tg/ gases
        Healium = 10, // Assmos - /tg/ gases
        Nitrium = 11, // Assmos - /tg/ gases
        Pluoxium = 12, // Assmos - /tg/ gases
        Hydrogen = 13, // Assmos - /tg/ gases
        HyperNoblium = 14, // Assmos - /tg/ gases
        ProtoNitrate = 15, // Assmos - /tg/ gases
        Zauker = 16, // Assmos - /tg/ gases
        Halon = 17, // Assmos - /tg/ gases
        Helium = 18, // Assmos - /tg/ gases
        AntiNoblium = 19, // Assmos - /tg/ gases
    }
}
