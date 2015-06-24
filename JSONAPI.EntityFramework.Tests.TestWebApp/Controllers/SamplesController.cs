using System;
using System.Web.Http;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class SamplesController : ApiController
    {
        public IHttpActionResult GetSamples()
        {
            var s1 = new Sample
            {
                Id = "1",
                BooleanField = false,
                NullableBooleanField = false,
                SByteField = default(SByte),
                NullableSByteField = null,
                ByteField = default(Byte),
                NullableByteField = null,
                Int16Field = default(Int16),
                NullableInt16Field = null,
                UInt16Field = default(UInt16),
                NullableUInt16Field = null,
                Int32Field = default(Int32),
                NullableInt32Field = null,
                UInt32Field = default(Int32),
                NullableUInt32Field = null,
                Int64Field = default(Int64),
                NullableInt64Field = null,
                UInt64Field = default(UInt64),
                NullableUInt64Field = null,
                DoubleField = default(Double),
                NullableDoubleField = null,
                SingleField = default(Single),
                NullableSingleField = null,
                DecimalField = default(Decimal),
                NullableDecimalField = null,
                DateTimeField = default(DateTime),
                NullableDateTimeField = null,
                DateTimeOffsetField = default(DateTimeOffset),
                NullableDateTimeOffsetField = null,
                GuidField = default(Guid),
                NullableGuidField = null,
                StringField = default(String),
                EnumField = default(SampleEnum),
                NullableEnumField = null,
            };
            var s2 = new Sample
            {
                Id = "2",
                BooleanField = true,
                NullableBooleanField = true,
                SByteField = 123,
                NullableSByteField = 123,
                ByteField = 253,
                NullableByteField = 253,
                Int16Field = 32000,
                NullableInt16Field = 32000,
                UInt16Field = 64000,
                NullableUInt16Field = 64000,
                Int32Field = 2000000000,
                NullableInt32Field = 2000000000,
                UInt32Field = 3000000000,
                NullableUInt32Field = 3000000000,
                Int64Field = 9223372036854775807,
                NullableInt64Field = 9223372036854775807,
                UInt64Field = 9223372036854775808,
                NullableUInt64Field = 9223372036854775808,
                DoubleField = 1056789.123,
                NullableDoubleField = 1056789.123,
                SingleField = 1056789.123f,
                NullableSingleField = 1056789.123f,
                DecimalField = 1056789.123m,
                NullableDecimalField = 1056789.123m,
                DateTimeField = new DateTime(1776, 07, 04),
                NullableDateTimeField = new DateTime(1776, 07, 04),
                DateTimeOffsetField = new DateTimeOffset(new DateTime(1776, 07, 04), new TimeSpan(-5, 0, 0)),
                NullableDateTimeOffsetField = new DateTimeOffset(new DateTime(1776, 07, 04), new TimeSpan(-5, 0, 0)),
                GuidField = new Guid("6566F9B4-5245-40DE-890D-98B40A4AD656"),
                NullableGuidField = new Guid("3D1FB81E-43EE-4D04-AF91-C8A326341293"),
                StringField = "Some string 156",
                EnumField = SampleEnum.Value1,
                NullableEnumField = SampleEnum.Value2,
            };

            return Ok(new[] { s1, s2 });
        }
    }
}