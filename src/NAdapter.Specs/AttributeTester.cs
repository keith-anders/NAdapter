using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NAdapter.Test
{
    public class AttributeTester<T>
    {
        Action _prep;
        Func<AttributeSpecification> _specifier;
        Action _finish;
        Func<T> _member;
        Func<T, IEnumerable<Attribute>> _attributeGetter;

        public AttributeTester(Action actionToPrep, Func<AttributeSpecification> attributeSpecifier, Action actionToFinish, Func<T> member,
            Func<T, IEnumerable<Attribute>> attributeGetter)
        {
            _prep = actionToPrep;
            _specifier = attributeSpecifier;
            _finish = actionToFinish;
            _member = member;
            _attributeGetter = attributeGetter;
        }

        public void TestNewAttribute()
        {
            _prep();
            var attributes = _specifier();
            attributes.AddAttribute(() => new AdapterTestConvertedAttribute("new value"));
            _finish();
            T result = _member();
            var att = _attributeGetter(result).OfType<AdapterTestConvertedAttribute>().Single();
            att.Value.ShouldBe("new value");
        }

        public void TestHidingAttribute()
        {
            _prep();
            var attributes = _specifier();
            attributes.HideAttributesOfType<AdapterTestAttribute>();
            _finish();
            T result = _member();

            _attributeGetter(result).OfType<AdapterTestAttribute>().ShouldBeEmpty();
        }

        public void TestConvertingAttribute()
        {
            _prep();
            var attributes = _specifier();
            attributes.RegisterAttributeConversion<AdapterTestAttribute>(a => new AdapterTestConvertedAttribute(a.Name));
            _finish();
            T result = _member();
            var recoveredAttributes = _attributeGetter(result);

            recoveredAttributes
                            .Cast<AdapterTestConvertedAttribute>()
                            .ShouldHaveSingleItem()
                            .Value
                            .ShouldBe("test");
        }
    }
}
