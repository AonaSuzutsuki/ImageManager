function menus(num) {
    var pathslash = "";

    document.getElementById("menus").innerHTML = "\
    <ul id=\"dropmenu\">\
        <li><a href=" + resolvePath(num, "index.html") + ">Top</a></li>\
        <li><a href=" + resolvePath(num, "index.html") + ">Usage</a></li>\
        <li><a href=" + resolvePath(num, "development_basic/index.html") + ">Classes</a>\
            <ul>\
                <li><a href=" + resolvePath(num, "classes/ClusterableFileStream/ClusterableFileStream.html") + ">ClusterableFileStream</a>\
                    <ul>\
                        <li><a href=" + resolvePath(num, "classes/ClusterableFileStream/IO/IO.html") + ">IO</a>\
                            <ul>\
                                <li><a href=" + resolvePath(num, "classes/ClusterableFileStream/IO/ClusterableFileStream.html") + ">ClusterableFileStream</a></li>\
                            </ul>\
                        </li>\
                    </ul>\
                </li>\
                <li><a href=" + resolvePath(num, "classes/FileManagerLib/FileManagerLib.html") + ">FileManagerLib</a>\
                    <ul>\
                        <li><a href=" + resolvePath(num, "classes/FileManagerLib/Crypto/Crypto.html") + ">Crypto</a>\
                            <ul>\
                                <li><a href=" + resolvePath(num, "classes/FileManagerLib/Crypto/Sha256.html") + ">Sha256</a></li>\
                            </ul>\
                        </li>\
                        <li><a href=" + resolvePath(num, "classes/FileManagerLib/Dat/Dat.html") + ">Dat</a>\
                            <ul>\
                                <li><a href=" + resolvePath(num, "classes/FileManagerLib/Dat/DatFileManager.html") + ">DatFileManager</a></li>\
                            </ul>\
                        </li>\
                        <li><a href=" + resolvePath(num, "classes/FileManagerLib/Dat/Dat.html") + ">File</a>\
                            <ul>\
                                <li><a>ByteLoader</a></li>\
                                <li><a>DirectorySearcher</a></li>\
                                <li><a>Exceptions</a>\
                                    <ul>\
                                        <li><a>FileExistedException</a></li>\
                                    </ul>\
                                </li>\
                                <li><a>Json</a>\
                                    <ul>\
                                        <li><a>AbstractJsonResourceManager</a></li>\
                                        <li><a>JsonFileManager</a></li>\
                                        <li><a>JsonStructureManager</a></li>\
                                        <li><a>Structures</a></li>\
                                    </ul>\
                                </li>\
                            </ul>\
                        </li>\
                        <li><a>MimeType</a>\
                            <ul>\
                                <li><a>MimeTypeMap</a></li>\
                            </ul>\
                        </li>\
                        <li><a>Path</a>\
                            <ul>\
                                <li><a>PathItem</a></li>\
                                <li><a>PathSplitter</a></li>\
                            </ul>\
                        </li>\
                    </ul>\
                </li>\
            </ul>\
        </li>\
        <li><a href=" + resolvePath(num, "development_basic/index.html") + ">Interfaces</a>\
            <ul>\
                <li><a href=" + resolvePath(num, "classes/ClusterableFileStream/ClusterableFileStream.html") + ">ClusterableFileStream</a>\
                    <ul>\
                        <li><a href=" + resolvePath(num, "classes/ClusterableFileStream/IO/IO.html") + ">IO</a>\
                            <ul>\
                                <li><a href=" + resolvePath(num, "classes/ClusterableFileStream/IO/IClusterableFileStream.html") + ">IClusterableFileStream</a></li>\
                            </ul>\
                        </li>\
                    </ul>\
                </li>\
            </ul>\
        </li>\
     </ul>\
     <br clear=\"both\" />";
}

function resolvePath(num, pathFromRoot) {
    var pathslash = "";
    for (var i = 0; i <= num; i++) {
        if (i === 0)
            pathslash = "";
        else
            pathslash += "../";
    }

    return "\"" + pathslash + pathFromRoot + "\"";
}

function openAndClose($id) {
    $($id).slideToggle();

}