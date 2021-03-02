using Microsoft.CSharp.RuntimeBinder;
using Shouldly;
using System;

namespace NAdapter.Test
{
    /// <summary>
    /// Collection of convenience functions for test fixtures
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Begins a specification
        /// </summary>
        /// <typeparam name="T">Type of component to adapt</typeparam>
        /// <returns>Specification</returns>
        protected Specification<T> Specify<T>() where T : class => Specification.New<T>();

        /// <summary>
        /// Builds an adapter
        /// </summary>
        /// <typeparam name="TComponent">Type of component to adapt</typeparam>
        /// <param name="component">Component to adapt</param>
        /// <returns>Adapter</returns>
        protected IAdapter<TComponent> BuildAdapter<TComponent>(TComponent component)
            where TComponent : class => Specify<TComponent>().Finish().Create(component);
        
        /// <summary>
        /// Asserts that running an action throws a missing member exception
        /// </summary>
        /// <param name="a">Action</param>
        protected void AssertThrowsMissingMember(Action a)
        {
            try
            {
                a();
                false.ShouldBeTrue("Should have thrown a missing member exception.");
            }
            catch (RuntimeBinderException e)
            {
                e.Message.ShouldContain("cannot be");
            }
        }

        /// <summary>
        /// Asserts that running an action throws an exception related to protection level
        /// </summary>
        /// <param name="a">Action</param>
        protected void AssertThrowsProtectionLevel(Action a)
        {
            try
            {
                a();
                false.ShouldBeTrue("Should have thrown an exception when trying to access restricted member.");
            }
            catch (RuntimeBinderException e)
            {
                e.Message.ShouldMatch("(protection level|inaccessible)");
            }
        }
    }

    /// <summary>
    /// Collection of convenience functions for test fixtures
    /// </summary>
    /// <typeparam name="T">The type of component being adapted in this test fixture</typeparam>
    public abstract class TestBase<T>: TestBase where T: class
    {
        /// <summary>
        /// Specification for the adapter
        /// </summary>
        protected Specification<T> Spec { get; set; }

        /// <summary>
        /// Makes an adapter and returns it as dynamic
        /// </summary>
        /// <param name="component">Component to adapt</param>
        /// <param name="keys">Keys for field initializers</param>
        /// <returns>Adapter</returns>
        protected dynamic GetDynamic(T component = default(T))
            => Spec.Finish().Create(component);

        /// <summary>
        /// Makes an adapter and returns it as IAdapter
        /// </summary>
        /// <param name="component">Component to adapt</param>
        /// <param name="keys">Keys for field initializers</param>
        /// <returns>Adapter</returns>
        protected virtual IAdapter<T> GetAdapter(T component = default(T))
            => Spec.Finish().Create(component);

        public TestBase() => Spec = Specify<T>();
    }
}
