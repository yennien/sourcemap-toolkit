﻿using Xunit;
using System.IO;
using System;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{
	public class WebpackTestProvider : ISourceMapProvider, ISourceCodeProvider
	{
		private static readonly string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "webpackapp");

		private StreamReader GetStreamOrNull(string fileName)
		{
			string filePath = Path.Combine(basePath, fileName);
			if (File.Exists(filePath))
			{
				return new StreamReader(File.OpenRead(filePath));
			}

			return null;
		}

		public StreamReader GetSourceCode(string sourceUrl)
		{
			return GetStreamOrNull(Path.GetFileName(sourceUrl));
		}

		public StreamReader GetSourceMapContentsForCallstackUrl(string sourceUrl)
		{
			return GetStreamOrNull($"{Path.GetFileName(sourceUrl)}.map");
		}
	}

	public class StackTraceDeminifierWebpackEndToEndTests
	{
        private StackTraceDeminifier GetStackTraceDeminifierWithDependencies()
		{
			var provider = new WebpackTestProvider();
			return StackTraceDeminfierFactory.GetStackTraceDeminfier(provider, provider);
        }

		[Fact]
		public void DeminifyStackTrace_MinifiedStackTrace_CorrectDeminificationWhenPossible()
		{
			// Arrange
			StackTraceDeminifier stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
			string chromeStackTrace = @"TypeError: Cannot read property 'nonExistantmember' of undefined
    at t.onButtonClick (http://localhost:3000/js/bundle.ffe51781aee314a37903.min.js:1:3573)
    at Object.sh (https://cdnjs.cloudflare.com/ajax/libs/react-dom/16.8.6/umd/react-dom.production.min.js:164:410)";
			string deminifiedStackTrace = @"TypeError: Cannot read property 'nonExistantmember' of undefined
  at _this.onButtonClick in webpack:///./components/App.tsx:11:46
  at Object.sh in https://cdnjs.cloudflare.com/ajax/libs/react-dom/16.8.6/umd/react-dom.production.min.js:164:410";

			// Act
			DeminifyStackTraceResult results = stackTraceDeminifier.DeminifyStackTrace(chromeStackTrace);

			// Assert
			Assert.Equal(deminifiedStackTrace.Replace("\r", ""), results.ToString().Replace("\r", ""));
		}
	}
}