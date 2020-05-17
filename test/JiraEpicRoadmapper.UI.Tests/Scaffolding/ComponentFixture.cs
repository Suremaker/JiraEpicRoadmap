using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using LightBDD.Framework;
using Microsoft.AspNetCore.Components;

namespace JiraEpicRoadmapper.UI.Tests.Scaffolding
{
    public class ComponentFixture<T> : ComponentTestFixture where T : class, IComponent
    {
        private State<IRenderedComponent<T>> _component;
        private readonly Dictionary<string, ComponentParameter> _parameters = new Dictionary<string, ComponentParameter>();

        public void When_I_render_it()
        {
            Component = RenderedComponentWithParameters();
        }

        protected IRenderedComponent<T> RenderedComponentWithParameters() => RenderComponent<T>(_parameters.Values.ToArray());

        public void Given_it_has_parameter_value(string parameter, object value) => WithComponentParameter(ComponentParameter.CreateParameter(parameter, value));

        protected IRenderedComponent<T> Component
        {
            get => _component.GetValue(nameof(Component));
            set => _component = new State<IRenderedComponent<T>>(value);
        }

        protected void WithComponentParameter(ComponentParameter parameter) => _parameters[parameter.Name ?? Guid.NewGuid().ToString()] = parameter;

        public void Then_I_should_see_content(string content)
        {
            Component.MarkupMatches(content);
        }
    }
}