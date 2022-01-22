namespace SettingsWorker.Changes;

public enum ChangesTokenType
{
    None,
    NextIsChange,
    Stop,
    // изменение обычно заканчивается чем товроде .". или ."; использовать только как вспомогательный инструмент
    // очень часто из-за этого были ошибки.
    Ancor,

}