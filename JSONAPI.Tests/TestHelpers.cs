﻿using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using JSONAPI.Json;

namespace JSONAPI.Tests
{
    internal static class TestHelpers
    {
        public static string ReadEmbeddedFile(string path)
        {
            var resourcePath = "JSONAPI.Tests." + path.Replace("\\", ".").Replace("/", ".");
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath))
            {
                if (stream == null) throw new Exception("Could not find a file at the path: " + path);
                return new StreamReader(stream).ReadToEnd();
            }
        }

        public static void StreamContentsMatchFixtureContents(MemoryStream stream, string fixtureFileName)
        {
            var output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            var expectedJson = ReadEmbeddedFile(fixtureFileName);
            var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            output.Should().Be(minifiedExpectedJson);
        }
    }
}
