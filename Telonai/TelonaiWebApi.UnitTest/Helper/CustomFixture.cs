using System;
using System.Collections.Generic;
using AutoFixture.Kernel;
using AutoFixture;
using AutoFixture.Xunit2;
using AutoFixture.AutoMoq;

namespace TelonaiWebAPI.UnitTest.Helper
{
    
    public static class CustomFixture
    {
        public static Fixture Create()
        {
        var fixture = (Fixture)new Fixture().Customize(new AutoMoqCustomization());
        fixture.Customize<DateOnly>(composer => composer.FromFactory<DateTime>(DateOnly.FromDateTime));
        return fixture;
        }
    }
    public class CustomAutoDataAttribute : AutoDataAttribute 
    { 
        public CustomAutoDataAttribute() 
            : base(() => CustomFixture.Create()) 
        { 
        } 
    }
}
