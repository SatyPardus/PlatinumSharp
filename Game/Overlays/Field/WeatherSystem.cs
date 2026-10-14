using static PlatinumSharp.Game.Unported;

namespace PlatinumSharp.Game;

public sealed class WeatherSystem
{
    public const int ManagerOffset = 0x00;
    public const int CurrentWeatherOffset = 0x04;
    public const int NextWeatherOffset = 0x08;
    public const int StateOffset = 0x0C;
    public const int QueuedWeatherOffset = 0x10;
    public const int TaskOffset = 0x14;
    public const int Size = 0x18;

    public WeatherEffectManager? Manager;
    public int CurrentWeather;
    public int NextWeather;
    public int State;
    public int QueuedWeather;
    public SysTask? Task;
}

public static class FieldWeather
{
    private const int HeapIDFieldMap = 4;
    private const int WeatherSnowing = 5;
    private const int WeatherHeavySnow = 6;
    private const int WeatherBlizzard = 7;
    private const int WeatherSnowVariant = 0x15;
    private const int WeatherNone = 0x1F;
    private const int StateIdle = 6;
    private const int CommandLoad = 0;
    private const int CommandLoadForFadeIn = 1;
    private const int CommandFadeIn = 2;
    private const int CommandStart = 3;
    private const int CommandCrossFadeIn = 4;
    private const int CommandFadeOut = 5;
    private const int CommandCrossFadeOut = 7;
    private const int CommandUnload = 8;
    private const int EffectStateLoaded = 1;
    private const int EffectStateRunning = 3;
    private const int LayerObj = 0x10;

    private static void SendCommand(WeatherSystem weatherSystem, int command, int weather)
    {
        if (WeatherEffects.WeatherEffectManager_Command(weatherSystem.Manager!, command, weather) == 0)
        {
            ErrorHandling.GF_AssertFail();
        }
    }

    private static void FinishChange(SysTask task, WeatherSystem weatherSystem)
    {
        weatherSystem.State = StateIdle;
        weatherSystem.Task = null;
        SysTask.SysTask_Done(task);
        if (weatherSystem.QueuedWeather != WeatherNone)
        {
            WeatherSystem_Change(weatherSystem, weatherSystem.QueuedWeather);
            weatherSystem.QueuedWeather = WeatherNone;
        }
    }

    // OFFSET 0x021D5EB8
    public static WeatherSystem WeatherSystem_New(FieldSystem fieldSystem)
    {
        WeatherSystem weatherSystem = new();
        weatherSystem.Manager = WeatherEffects.WeatherEffectManager_New(fieldSystem);
        weatherSystem.CurrentWeather = 0;
        weatherSystem.NextWeather = 0;
        weatherSystem.State = StateIdle;
        weatherSystem.Task = null;
        weatherSystem.QueuedWeather = WeatherNone;
        GXLayers.GXLayers_EngineAToggleLayers(LayerObj, 1);
        return weatherSystem;
    }

    // OFFSET 0x021D5EF8
    public static void WeatherSystem_Free(object? weatherSystemObject)
    {
        WeatherSystem weatherSystem = (WeatherSystem)weatherSystemObject!;
        if (weatherSystem.Task != null)
        {
            SysTask.SysTask_Done(weatherSystem.Task);
        }
        WeatherEffects.WeatherEffectManager_Free(ref weatherSystem.Manager);
        weatherSystem.Manager = null;
        weatherSystem.CurrentWeather = 0;
        weatherSystem.NextWeather = 0;
        weatherSystem.State = 0;
        weatherSystem.QueuedWeather = 0;
        weatherSystem.Task = null;
    }

    // OFFSET 0x021D5F24
    public static void WeatherSystem_Set(object? weatherSystemObject, int weather)
    {
        WeatherSystem weatherSystem = (WeatherSystem)weatherSystemObject!;
        if (weatherSystem.State != StateIdle)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (weather >= WeatherNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (weatherSystem.CurrentWeather != weather)
        {
            SendCommand(weatherSystem, CommandUnload, weatherSystem.CurrentWeather);
            SendCommand(weatherSystem, CommandLoad, weather);
            SendCommand(weatherSystem, CommandStart, weather);
            weatherSystem.CurrentWeather = weather;
        }
    }

    // OFFSET 0x021D5F7C
    public static bool WeatherSystem_Change(object? weatherSystemObject, int weather)
    {
        WeatherSystem weatherSystem = (WeatherSystem)weatherSystemObject!;
        if (weather >= WeatherNone)
        {
            ErrorHandling.GF_AssertFail();
        }
        if (weatherSystem.State != StateIdle)
        {
            weatherSystem.QueuedWeather = weather;
            return true;
        }
        if (weatherSystem.CurrentWeather == weather)
        {
            return true;
        }
        if (weatherSystem.Task != null)
        {
            ErrorHandling.GF_AssertFail();
        }
        weatherSystem.NextWeather = weather;
        if (!WeatherSystem_CanCrossFade(weatherSystem.CurrentWeather, weather))
        {
            weatherSystem.State = 0;
            weatherSystem.Task = SysTask.SysTask_Start(WeatherSystem_FadeTask, weatherSystem, 0);
        }
        else
        {
            weatherSystem.State = 0;
            weatherSystem.Task = SysTask.SysTask_Start(WeatherSystem_CrossFadeTask, weatherSystem, 0);
        }
        return true;
    }

    // OFFSET 0x021D5FE0
    public static int WeatherSystem_GetCurrentWeather(WeatherSystem weatherSystem)
    {
        return weatherSystem.CurrentWeather;
    }

    // OFFSET 0x021D5FE4
    private static void WeatherSystem_FadeTask(SysTask task, object? param)
    {
        WeatherSystem weatherSystem = (WeatherSystem)param!;
        switch (weatherSystem.State)
        {
            case 0:
                SendCommand(weatherSystem, CommandFadeOut, weatherSystem.CurrentWeather);
                weatherSystem.State++;
                break;
            case 1:
                if (WeatherEffects.WeatherEffectManager_GetEffectState(weatherSystem.Manager!, weatherSystem.CurrentWeather) != EffectStateRunning)
                {
                    SendCommand(weatherSystem, CommandUnload, weatherSystem.CurrentWeather);
                    weatherSystem.State++;
                }
                break;
            case 2:
                SendCommand(weatherSystem, CommandLoadForFadeIn, weatherSystem.NextWeather);
                weatherSystem.State++;
                break;
            case 3:
                if (WeatherEffects.WeatherEffectManager_GetEffectState(weatherSystem.Manager!, weatherSystem.NextWeather) != EffectStateLoaded)
                {
                    weatherSystem.State++;
                }
                break;
            case 4:
                SendCommand(weatherSystem, CommandFadeIn, weatherSystem.NextWeather);
                weatherSystem.State++;
                weatherSystem.CurrentWeather = weatherSystem.NextWeather;
                weatherSystem.NextWeather = 0;
                break;
            case 5:
                FinishChange(task, weatherSystem);
                break;
        }
    }

    // OFFSET 0x021D60B4
    private static void WeatherSystem_CrossFadeTask(SysTask task, object? param)
    {
        WeatherSystem weatherSystem = (WeatherSystem)param!;
        switch (weatherSystem.State)
        {
            case 0:
                SendCommand(weatherSystem, CommandLoadForFadeIn, weatherSystem.NextWeather);
                weatherSystem.State++;
                break;
            case 1:
                if (WeatherEffects.WeatherEffectManager_GetEffectState(weatherSystem.Manager!, weatherSystem.NextWeather) != EffectStateLoaded)
                {
                    weatherSystem.State++;
                }
                break;
            case 2:
                SendCommand(weatherSystem, CommandCrossFadeOut, weatherSystem.CurrentWeather);
                SendCommand(weatherSystem, CommandCrossFadeIn, weatherSystem.NextWeather);
                weatherSystem.State++;
                break;
            case 3:
                if (WeatherEffects.WeatherEffectManager_GetEffectState(weatherSystem.Manager!, weatherSystem.CurrentWeather) != EffectStateRunning)
                {
                    SendCommand(weatherSystem, CommandUnload, weatherSystem.CurrentWeather);
                    weatherSystem.State++;
                }
                break;
            case 4:
                weatherSystem.CurrentWeather = weatherSystem.NextWeather;
                weatherSystem.NextWeather = 0;
                FinishChange(task, weatherSystem);
                break;
        }
    }

    // OFFSET 0x021D6178
    private static bool WeatherSystem_CanCrossFade(int currentWeather, int nextWeather)
    {
        bool canCrossFade = false;
        switch (currentWeather)
        {
            case WeatherSnowing:
                if (nextWeather == WeatherHeavySnow || nextWeather == WeatherSnowVariant || nextWeather == WeatherBlizzard)
                {
                    canCrossFade = true;
                }
                break;
            case WeatherHeavySnow:
                if (nextWeather == WeatherSnowing || nextWeather == WeatherSnowVariant || nextWeather == WeatherBlizzard)
                {
                    canCrossFade = true;
                }
                break;
            case WeatherSnowVariant:
                if ((uint)(nextWeather - WeatherSnowing) <= WeatherBlizzard - WeatherSnowing)
                {
                    canCrossFade = true;
                }
                break;
            case WeatherBlizzard:
                if (nextWeather == WeatherSnowing || nextWeather == WeatherSnowVariant || nextWeather == WeatherHeavySnow)
                {
                    canCrossFade = true;
                }
                break;
        }
        return canCrossFade;
    }
}
