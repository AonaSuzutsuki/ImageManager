function menus(num) {
    var pathslash = "";

    document.getElementById("menus").innerHTML = "\
    <ol id=\"dropmenu\">\
        <li><a href=" + resolvePath(num, "index.html") + ">トップ</a></li>\
        <li><a href=" + resolvePath(num, "index.html") + ">使用マニュアル</a></li>\
        <li><a href=" + resolvePath(num, "development_basic/index.html") + ">クラス</a>\
            <ol>\
                <li><a href=" + resolvePath(num, "classes/ClusterableFileStream/ClusterableFileStream.html") + ">ClusterableFileStream</a>\
                    <ol>\
                        <li><a href=" + resolvePath(num, "classes/ClusterableFileStream/IO/IO.html") + ">IO</a>\
                            <ol>\
                                <li><a href=" + resolvePath(num, "classes/ClusterableFileStream/IO/ClusterableFileStream.html") + ">ClusterableFileStream</a></li>\
                            </ol>\
                        </li>\
                    </ol>\
                </li>\
                <li><a href=" + resolvePath(num, "classes/FileManagerLib/FileManagerLib.html") + ">FileManagerLib</a>\
                    <ol>\
                        <li><a href=" + resolvePath(num, "classes/FileManagerLib/Crypto/Crypto.html") + ">Crypto</a>\
                            <ol>\
                                <li><a href=" + resolvePath(num, "classes/FileManagerLib/Crypto/Sha256.html") + ">Sha256</a></li>\
                            </ol>\
                        </li>\
                        <li><a href=" + resolvePath(num, "classes/FileManagerLib/Dat/Dat.html") + ">Dat</a>\
                            <ol>\
                                <li><a href=" + resolvePath(num, "classes/FileManagerLib/Dat/DatFileManager.html") + ">DatFileManager</a></li>\
                            </ol>\
                        </li>\
                    </ol>\
                </li>\
            </ol>\
        </li>\
     </ol>\
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