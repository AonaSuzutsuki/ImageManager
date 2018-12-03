using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileManagerLib.File.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagerLib.File.Json.Tests
{
    [TestClass()]
    public class JsonStructureManagerTests
    {

        public JsonStructureManager MakeJsonManager(string json = "")
        {
            var jsonManager = new JsonStructureManager(json, true);
            if (json == "")
            {
                jsonManager.CreateDirectory(jsonManager.NextDirectoryId, 0, "test");
                jsonManager.CreateFile(jsonManager.NextFileId, 0, "test.txt", 0, CommonCoreLib.Crypto.Sha256.GetSha256(new byte[] { 0 }), new Dictionary<string, string>()
                {
                    { "MimeType", "text/plain" }
                });
            }
            return jsonManager;
        }

        [TestMethod()]
        public void JsonStructureManagerTest()
        {
            var exp = "{\"directory\":[{\"id\":1,\"parent\":0,\"name\":\"test\"}],\"file\":[{\"id\":1,\"parent\":0,\"name\":\"test.txt\",\"location\":0,\"hash\":\"6E340B9CFFB37A989CA544E6BB780A2C78901D3FB33738768511A30617AFA01D\",\"additional\":{\"MimeType\":\"text/plain\"}}],\"isCheckHash\":true}";

            var jsonManager = MakeJsonManager();

            var json = jsonManager.ToString();
            Assert.AreEqual(exp, json);
        }

        [TestMethod()]
        public void JsonStructureManagerTest2()
        {
            var baseData = "{\"directory\":[{\"id\":1,\"parent\":0,\"name\":\"test\"}],\"file\":[{\"id\":1,\"parent\":0,\"name\":\"test.txt\",\"location\":0,\"hash\":\"6E340B9CFFB37A989CA544E6BB780A2C78901D3FB33738768511A30617AFA01D\",\"additional\":{\"MimeType\":\"text/plain\"}}],\"isCheckHash\":true}";
            var exp = "{\"directory\":[{\"id\":1,\"parent\":0,\"name\":\"test\"},{\"id\":2,\"parent\":0,\"name\":\"test2\"}],\"file\":[{\"id\":1,\"parent\":0,\"name\":\"test.txt\",\"location\":0,\"hash\":\"6E340B9CFFB37A989CA544E6BB780A2C78901D3FB33738768511A30617AFA01D\",\"additional\":{\"MimeType\":\"text/plain\"}}],\"isCheckHash\":true}";

            var jsonManager = MakeJsonManager(baseData);
            jsonManager.CreateDirectory(jsonManager.NextDirectoryId, 0, "test2");

            var json = jsonManager.ToString();
            Assert.AreEqual(exp, json);
        }

        [TestMethod()]
        public void ChangeDirectoryTest()
        {
            var exp = "{\"directory\":[{\"id\":1,\"parent\":0,\"name\":\"fixedTest\"}],\"file\":[{\"id\":1,\"parent\":0,\"name\":\"test.txt\",\"location\":0,\"hash\":\"6E340B9CFFB37A989CA544E6BB780A2C78901D3FB33738768511A30617AFA01D\",\"additional\":{\"MimeType\":\"text/plain\"}}],\"isCheckHash\":true}";

            var jsonManager = MakeJsonManager();
            jsonManager.ChangeDirectory(1, new DirectoryStructure()
            {
                Id = 1,
                Parent = 0,
                Name = "fixedTest"
            });
            jsonManager.ChangeDirectory(2, new DirectoryStructure()
            {
                Id = 2,
                Parent = 0,
                Name = "subDir"
            });

            var json = jsonManager.ToString();
            Assert.AreEqual(exp, json);
        }

        [TestMethod()]
        public void ChangeFileTest()
        {
            var exp = "{\"directory\":[{\"id\":1,\"parent\":0,\"name\":\"test\"}],\"file\":[{\"id\":1,\"parent\":0,\"name\":\"test2.txt\",\"location\":12,\"hash\":\"6E340B9CFFB37A989CA544E6BB780A2C78901D3FB33738768511A30617AFA01D\",\"additional\":{\"MimeType\":\"text/plain\"}}],\"isCheckHash\":true}";

            var jsonManager = MakeJsonManager();
            jsonManager.ChangeFile(1, new FileStructure()
            {
                Id = 1,
                Parent = 0,
                Name = "test2.txt",
                Location = 12,
                Hash = "6E340B9CFFB37A989CA544E6BB780A2C78901D3FB33738768511A30617AFA01D",
                Additional = new Dictionary<string, string>()
                {
                    { "MimeType", "text/plain" }
                }
            });
            jsonManager.ChangeFile(2, new FileStructure()
            {
                Id = 2,
                Parent = 0,
                Name = "test3.txt",
                Location = 24,
                Hash = "6E340B9CFFB37A989CA544E6BB780A2C78901D3FB33738768511A30617AFA01D",
                Additional = new Dictionary<string, string>()
                {
                    { "MimeType", "text/plain" }
                }
            });

            var json = jsonManager.ToString();
            Assert.AreEqual(exp, json);
        }
    }
}