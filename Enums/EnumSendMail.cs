using System.ComponentModel;

namespace ChatApp.Enum
{
    public enum EnumSenderAction
    {
        [Description("Register")]
        REGISTER = 1,
        [Description("Fogot password")]
        FOGOT_PASSWORD = 2, 
        [Description("Change password")]
        CHANGE_PASSWORD = 3,

    }
}
