
//========================================
//  URL & Certificate ����
var HOST = 'localhost';
var PORT = 8000;
var CERTIFICATE = "qs200811";

//========================================
//  �� �ִ� ���� ����
var ROOM_MAX = 6;
var SQUARE_MAX = 20;

//========================================
//  ���� ���� ����
var Sockets = new Map();
var idx = 0;

//========================================
//���忡 �ִ� �÷��̾�
var SquarePlayers = new Map();
//��Ƽ�� �ִ� �÷��̾�
var Rooms = new Map();

//========================================
//���忡 �ִ� ����ȭ ��ü
var SquareSyncObjs = new Map();

//========================================
//�ʿ� ���
var colors = require('colors');
var io = require('socket.io').listen(PORT);

//var txt = "{'Accountpk': 1}";
//var rep = txt.replace(/'/g, '\"'); //replace(\\'\\g,'\"');
//var jsPaser = JSON.parse(rep);

//=======================================
//���� ���� ó��
io.on('connection', function (client) {

    client.once('disconnect', function (data)
    {
        DelPlayerToRoom(client);
        LeaveSquare(client);
        RemoveClient(client);
    });
    client.once('disconnecting', function (data) {

    });
    client.on('error', function (err)
    {
        Log('[ERR]', JSON.stringify(err));
    });



    client.on('entersquare', function () {
        EnterSquare(client);
    });

    client.on('leavesquare', function () {
        LeaveSquare(client);
    });

    client.on('showplayer', function (data) {
        ShowPlayer(client);
    });
    client.on('hideplayer', function (data) {
        HidePlayer(client);
    });


    client.on('move', function (data) {
        SendMove(client, data);
    });

    client.on('voice', function (json, voice) {

        SendVoice(client, json, voice);

    });

    client.on('obj_move', function (syncid,json) {

        SendObjectMove(client, syncid, json);

    });

    client.on('line_draw', function (json) {

        SendLineDraw(client, json);

    });
    client.on('erase_draw', function (json) {

        SendEraseDraw(client);

    });



    client.on('createroom', function (type, pw) {
        CreateRoom(client, type, pw);
    });
    client.on('roomlist', function () {
        RoomList(client);
    });
    client.on('enterroom', function (roomindex, pw) {
        EnterRoom(client, roomindex, pw);
    });
    client.on('leaveroom', function () {
        LeaveRoom(client);
    });
    client.on('roomready', function (ready) {
        RoomReady(client, ready);
    });
    client.on('roomstart', function () {
        RoomStart(client);
    });
  


    if (!CheckCertificate(client)) {
        client.disconnect();
        return;
    }

    LoginSuccess(client);
    
});

//=======================================
// �α� �Լ�
function Log(msg) {
    console.log(msg);
}
//=======================================
// ���� üũ �Լ�
function CheckCertificate(client) {

    if (base64decode(client.handshake.query.certificate) === CERTIFICATE) {
        return true;
    }
    //if (client.handshake.query.certificate === CERTIFICATE) {
    //    return true;
    //}

    return false;
}



//=======================================
// �α��� ���� ó��
function LoginSuccess(client) {
    AddClient(client);
    SendLogin(client);
}
//=======================================
// �α��� ���� ����
function SendLogin(client) {
    client.emit('login', 0, idx);
}
//=======================================
// �α��� ���� �߰� �Լ�
function AddClient(client) {

    var q_id = base64decode(client.handshake.query.id);
    Sockets.set(client.id, CreateUserInfo(client.id, q_id, ++idx));
    var log = '[ENTER] Connect : ' + Sockets.size + ' [INFO] idx : ' + idx + ' / id : ' + q_id + ' / address : ' + client.request.connection.remoteAddress + ':' + client.request.connection.remotePort;
    Log(log);
}
//=======================================
// �α׾ƿ� ���� ���� �Լ�
function RemoveClient(client) {
    var player = Sockets.get(client.id);
    if (player !== null && player !== undefined) {
        io.sockets.emit('leaveroomntf', player.Accountpk);
        Sockets.delete(client.id);
    }

    var q_id = base64decode(client.handshake.query.id);
    var log = '[EXIT]  Connect : ' + Sockets.size + ' [INFO] idx : ' + idx + ' / id : ' + q_id + ' / address : ' + client.request.connection.remoteAddress + ':' + client.request.connection.remotePort;
    Log(log.red.bold);
}


//=======================================
// ���� ���� ���� �Լ�
function CreateUserInfo(argSockID, argID, argIDX) {
    var sockData = new Object();

    sockData.userData = new Object();
    sockData.sockid = argSockID;
    sockData.id = argID;
    sockData.Accountpk = argIDX;
    sockData.IsVisible = 1;
    sockData.roomindex = 0;
    sockData.equipInfo = GetBaseEquipInfo();
    sockData.playerPos = GetBasePlayerPosition(argIDX);
    sockData.PlayerID = argID;//String(argIDX);
    sockData.PublicIP = '';
    sockData.PublicPort = 0;
    sockData.PrivateIP = '';
    sockData.privatePort = 0;
    return sockData;
}
//=======================================
// ���� ��ġ �ʱ�ȭ
function GetBasePlayerPosition(argIDX) {
    var playerPos = new Object();

    playerPos.Accountpk = argIDX;
    playerPos.IsVisible = 1;

    playerPos.AvtPos = new Object();
    playerPos.AvtPos.x = 0;
    playerPos.AvtPos.y = 0;
    playerPos.AvtPos.z = 0;

    playerPos.AvtRot = new Object();
    playerPos.AvtRot.x = 0;
    playerPos.AvtRot.y = 0;
    playerPos.AvtRot.z = 0;

    playerPos.HeadPos = new Object();
    playerPos.HeadPos.x = 0;
    playerPos.HeadPos.y = 0;
    playerPos.HeadPos.z = 0;

    playerPos.HeadRot = new Object();
    playerPos.HeadRot.x = 0;
    playerPos.HeadRot.y = 0;
    playerPos.HeadRot.z = 0;

    playerPos.LHandPos = new Object();
    playerPos.LHandPos.x = 0;
    playerPos.LHandPos.y = 0;
    playerPos.LHandPos.z = 0;

    playerPos.LHandRot = new Object();
    playerPos.LHandRot.x = 0;
    playerPos.LHandRot.y = 0;
    playerPos.LHandRot.z = 0;

    playerPos.RHandPos = new Object();
    playerPos.RHandPos.x = 0;
    playerPos.RHandPos.y = 0;
    playerPos.RHandPos.z = 0;

    playerPos.RHandRot = new Object();
    playerPos.RHandRot.x = 0;
    playerPos.RHandRot.y = 0;
    playerPos.RHandRot.z = 0;

    return playerPos;
}

//=======================================
// ���� �Ӽ� �ʱ�ȭ
function GetBaseEquipInfo() {
    var avataData = new Object();
    avataData.Gender = 0;
    avataData.HairTypeNumber = 0;
    avataData.HairColorNumber = 0;
    avataData.AccessoryTypeNumber = 0;
    avataData.AccessoryColorNumber = 0;
    avataData.ClothesTypeNumber = 0;
    avataData.ClothesColorNumber = 0;
    avataData.SkinColorNumber = 0;
    return avataData;
}



//=======================================
//���� �÷��̾� ����
function AddSquarePlayer(client) {
    SquarePlayers.set(client.id, Sockets.get(client.id));

}
//=======================================
//���� �÷��̾� ����
function DelSquarePlayer(client) {
    SquarePlayers.delete(client.id);
}

//=======================================
//�÷��̾ ���忡 ����
//1.������ �÷��̾�� �ش���� �÷��̾�� ������ ����
//2.���忡 �ִ� �÷��̾�鿡�� ������ �÷��̾� ���� ����
//3.�ش� Ÿ�� �濡 �÷��̾� �߰�
function EnterSquare(client) {

    var exist = SquarePlayers.has(client.id);

    //1.������ �÷��̾�� �ش���� �÷��̾�� ������ ����
    var arr = [];
    SquarePlayers.forEach(function (item, key, mapObj) {
        if (key !== client.id) {
            arr.push(item);
        }
    });
    var rmInfo = new Object();
    rmInfo.result = exist === true ? 1001 : 0;
    rmInfo.roomindex = 0;
    rmInfo.type = 0;
    rmInfo.hidden = 0;
    rmInfo.curplayer = arr.length;
    rmInfo.maxplayer = SQUARE_MAX;
    rmInfo.bossid = '';
    rmInfo.playercount = arr.length;
    rmInfo.players = arr;
    rmInfo.syncobjs = Array.from(SquareSyncObjs.values());

    client.emit('enterroom', JSON.stringify(rmInfo));

    if (exist) {
        return;
    }

    //2.���忡 �ִ� �÷��̾�鿡�� ������ �÷��̾� ���� ����
    EnterSquareNTF(client);

    //3.�ش� Ÿ�� �濡 �÷��̾� �߰�
    AddSquarePlayer(client);
}

//=======================================
//�ش� Ÿ���� �濡 �ִ� �÷��̾�鿡�� ������ �÷��̾� ���� ����
function EnterSquareNTF(client) {

    var player = Sockets.get(client.id);

    if (player !== null && player !== undefined) {

        var data = JSON.stringify(player);
       
        SquarePlayers.forEach(function (item, key, mapObj) {
            if (key !== client.id) {
                io.to(key).emit('enterroomntf', data);

            }
        });
    }
}

//=======================================
//���忡�� ����ó��
//1.���� map ���� �÷��̾� ����.
//2.�÷��̾�� ���� ��û�� ����
//3.���� �÷��̾�鿡�� ������ �÷��̾� �˸�
function LeaveSquare(client) {

    var player = SquarePlayers.get(client.id);

    if (player !== null && player !== undefined) {
        //1.���� map ���� �÷��̾� ����.
        DelSquarePlayer(client);

        //2.���� �÷��̾�鿡�� ������ �÷��̾� �˸�
        SquarePlayers.forEach(function (sock, key, mapObj) {

            if (sock.Accountpk !== player.Accountpk && sock.roomindex === 0) {
                io.to(key).emit('leaveroomntf', player.Accountpk);
            }

        });

        //3.�÷��̾�� ���� ��û�� ����
        //"0:����
        //1002:���忡 �������� �ʾҽ��ϴ�.
        //1201:�濡 �������� �ʾҽ��ϴ�."
        client.emit('leaveroom', 0);
    }
    else {
        client.emit('leaveroom', 1002);
    }

}


//=======================================
//�÷��̾� Show
function ShowPlayer(client) {
    var player = Sockets.get(client.id);
    if (player === undefined || player === null) {
        return;
    }
    player.IsVisible = 1;
    Sockets.set(client.id, player);

    var roomindex = player.roomindex;
    if (roomindex !== 0) {
        var rmInfo = Rooms.get(roomindex);
        if (rmInfo !== undefined && rmInfo !== null) {
            rmInfo.players.forEach(function (sock, key, mapObj) {
                var sockItem = Sockets.get(sock.id);
                if (sockItem !== undefined || socketItem !== null) {
                    if (sockItem.Accountpk !== player.Accountpk) {
                        sock.emit('showplayer', player.Accountpk);
                    }
                }
            });
        }
    }
    else {
        SquarePlayers.forEach(function (sock, key, mapObj) {
            if (sock.Accountpk !== player.Accountpk && sock.roomindex === 0) {
                io.to(key).emit('showplayer', player.Accountpk);
            }
        });
    }
}

//=======================================
//�÷��̾� Hide 
function HidePlayer(client) {
    var player = Sockets.get(client.id);
    if (player === undefined || player === null) {
        return;
    }
    player.IsVisible = 0;
    Sockets.set(client.id, player);

    var roomindex = player.roomindex;
    if (roomindex !== 0) {
        var rmInfo = Rooms.get(roomindex);
        if (rmInfo !== undefined && rmInfo !== null) {
            rmInfo.players.forEach(function (sock, key, mapObj) {
                var sockItem = Sockets.get(sock.id);
                if (sockItem !== undefined || socketItem !== null) {
                    if (sockItem.Accountpk !== player.Accountpk) {
                        sock.emit('hideplayer', player.Accountpk);
                    }
                }
            });
        }
    }
    else {
        SquarePlayers.forEach(function (sock, key, mapObj) {
            if (sock.Accountpk !== player.Accountpk && sock.roomindex === 0) {
                io.to(key).emit('hideplayer', player.Accountpk);
            }
        });
    }
}





//=======================================
//������ ������ ����ȭ
function SendMove(client, data) {
    var player = Sockets.get(client.id);
    if (player === undefined || player === null) {
        return;
    }

    player.playerPos = JSON.parse(data.replace(/'/g, '\"'));

    Sockets.set(client.id, player);

    var roomindex = player.roomindex;
    if (roomindex !== 0) {
        var rmInfo = Rooms.get(roomindex);
        if (rmInfo !== undefined && rmInfo !== null) {
            rmInfo.players.forEach(function (sock, key, mapObj) {
                var sockItem = Sockets.get(sock.id);
                if (sockItem !== undefined || socketItem !== null) {
                    if (sockItem.Accountpk !== player.Accountpk) {
                        sock.emit('move', data);
                    }
                }
            });
        }
    }
    else {
        SquarePlayers.forEach(function (sock, key, mapObj) {
            if (sock.Accountpk !== player.Accountpk && sock.roomindex === 0) {
                io.to(key).emit('move', data);
            }
        });
    }
}




//=======================================
//���� ä�� ����
function SendVoice(client, json, voice)
{

    var player = Sockets.get(client.id);

    if (player === undefined || player === null) {

        return;

    }

    var roomindex = player.roomindex;

    if (roomindex !== 0) {

        var rmInfo = Rooms.get(roomindex);

        if (rmInfo !== undefined || rmInfo !== null) {

            rmInfo.players.forEach(function (sock, key, mapObj) {

                sock.emit('voice', json, voice);

            });
        }
    }
    else
    {

        Sockets.forEach(function (item, key, mapObj) {

            if (item.roomindex === 0) {

                io.to(key).emit('voice', json, voice);

            }

        });

    }

}

function SendObjectMove(client, syncid, data) {
    var player = Sockets.get(client.id);
    if (player === undefined || player === null) {
        return;
    }

    var roomindex = player.roomindex;

    AddSyncObjectOfRoom(roomindex, syncid, data);

    if (roomindex !== 0) {
        var rmInfo = Rooms.get(roomindex);
        if (rmInfo !== undefined && rmInfo !== null) {
            rmInfo.players.forEach(function (sock, key, mapObj) {
                var sockItem = Sockets.get(sock.id);
                if (sockItem !== undefined || socketItem !== null) {
                    if (sockItem.Accountpk !== player.Accountpk) {
                        sock.emit('obj_move', data);
                    }
                }
            });
        }
    }
    else {
        SquarePlayers.forEach(function (sock, key, mapObj) {
            if (sock.Accountpk !== player.Accountpk && sock.roomindex === 0) {
                io.to(key).emit('obj_move', data);
            }
        });
    }
}


function SendLineDraw(client, data) {
    var player = Sockets.get(client.id);
    if (player === undefined || player === null) {
        return;
    }

    var roomindex = player.roomindex;

    if (roomindex !== 0) {
        var rmInfo = Rooms.get(roomindex);
        if (rmInfo !== undefined && rmInfo !== null) {
            rmInfo.players.forEach(function (sock, key, mapObj) {
                var sockItem = Sockets.get(sock.id);
                if (sockItem !== undefined || socketItem !== null) {
                    if (sockItem.Accountpk !== player.Accountpk) {
                        sock.emit('line_draw', data);
                    }
                }
            });
        }
    }
    else {
        SquarePlayers.forEach(function (sock, key, mapObj) {
            if (sock.Accountpk !== player.Accountpk && sock.roomindex === 0) {
                io.to(key).emit('line_draw', data);
            }
        });
    }
}


function SendEraseDraw(client) {
    var player = Sockets.get(client.id);
    if (player === undefined || player === null) {
        return;
    }

    var roomindex = player.roomindex;

    if (roomindex !== 0) {
        var rmInfo = Rooms.get(roomindex);
        if (rmInfo !== undefined && rmInfo !== null) {
            rmInfo.players.forEach(function (sock, key, mapObj) {
                var sockItem = Sockets.get(sock.id);
                if (sockItem !== undefined || socketItem !== null) {
                    if (sockItem.Accountpk !== player.Accountpk) {
                        sock.emit('erase_draw', player.Accountpk);
                    }
                }
            });
        }
    }
    else {
        SquarePlayers.forEach(function (sock, key, mapObj) {
            if (sock.Accountpk !== player.Accountpk && sock.roomindex === 0) {
                io.to(key).emit('erase_draw', player.Accountpk);
            }
        });
    }
}

//=======================================
//��(��Ƽ) ���� �Լ�
function CreateRoom(client, type, pw) {
    var partyInfo = new Object();

    if (Sockets.get(client.id).roomindex !== 0) {
        //"0:����
        //1002:���忡 �������� �ʾҽ��ϴ�.
        //1103:party ������ ����"
        partyInfo.result = 1103;
    }
    else {

        LeaveSquare(client);

        var rmInfo = AddRoomList(client, type, pw);

        //"0:����
        //1002:���忡 �������� �ʾҽ��ϴ�.
        //1103:party ������ ����"
        partyInfo.result = 0;

        partyInfo.myParty = new Object();
        partyInfo.myParty.roomindex = rmInfo.roomindex;
        partyInfo.myParty.type = rmInfo.type;
        partyInfo.myParty.hidden = rmInfo.hidden;
        partyInfo.myParty.curplayer = rmInfo.curplayer;
        partyInfo.myParty.maxplayer = rmInfo.maxPlayer;
        partyInfo.myParty.bossid = rmInfo.bossid;
    }
    client.emit('createroom', 0, JSON.stringify(partyInfo));

    if (partyInfo.myParty !== null && partyInfo.myParty !== undefined) {
        var log = '[ROOM] Create ' + ' [INFO] roomidx : ' + partyInfo.myParty.roomindex + ' / bossid : ' + partyInfo.myParty.bossid;
        Log(log.yellow.bold);
    }
    
}
function AddRoomList(client, type, pw) {
    var info = new Object();

    var sockInfo = Sockets.get(client.id);
    sockInfo.roomindex = sockInfo.Accountpk;
    Sockets.set(client.id, sockInfo);

    info.roomindex = sockInfo.roomindex;
    info.type = type;
    if (pw) {
        info.hidden = 1;
    }
    else {
        info.hidden = 0;
    }
    info.pw = pw;
    info.maxPlayer = ROOM_MAX;
    info.bossid = Sockets.get(client.id).id;
    info.players = [];
    info.players.push(client);
    info.curplayer = info.players.length;
    info.maxplayer = ROOM_MAX;
    info.playercount = info.players.length;
    info.syncobjs = new Map();

    Rooms.set(info.roomindex, info);

    return info;
}


function RoomList(client) {
    var list = new Object();
    list.partyinfos = [];
    Rooms.forEach(function (item, key, mapObj) {

        var rmInfo = new Object();
        //"0:����
        //1002:���忡 �������� �ʾҽ��ϴ�.
        //1103:party ������ ����"
        //rmInfo.myParty = new Object();
        rmInfo.roomindex = item.roomindex;
        rmInfo.type = item.type;
        rmInfo.hidden = item.hidden;
        rmInfo.curplayer = item.curplayer;
        rmInfo.maxPlayer = item.maxplayer;
        rmInfo.bossid = item.bossid;
        list.partyinfos.push(rmInfo);
    });

    list.listCount = list.partyinfos.length;

    client.emit('roomlist', 0, JSON.stringify(list));
}


function EnterRoom(client, roomindex, pw) {

    var result = AddPlayerToRoom(client, roomindex, pw);

    var retVal = new Object();
    retVal.result = result;

    if (result === 0) {

        LeaveSquare(client);

        var rmInfo = Rooms.get(roomindex);

        retVal.roomindex = rmInfo.roomindex;
        retVal.type = rmInfo.type;
        retVal.hidden = rmInfo.hidden;
        retVal.curplayer = rmInfo.curplayer;
        retVal.maxplayer = rmInfo.maxplayer;
        retVal.bossid = rmInfo.bossid;
        retVal.playercount = rmInfo.playercount;
        retVal.players = [];
        retVal.syncobjs = Array.from(rmInfo.syncobjs.values());

        rmInfo.players.forEach(function (item, key, mapObj) {
            var player = Sockets.get(item.id);
            if (player !== null && player !== undefined) {
                retVal.players.push(player);
            }
        });

        client.emit('enterroom', JSON.stringify(retVal));

        var player = Sockets.get(client.id);
        if (player !== null && player !== undefined) {

            var data = JSON.stringify(player);
            rmInfo.players.forEach(function (item, key, mapObj) {
                if (item.id !== client.id) {
                    io.to(item.id).emit('enterroomntf', data);

                }
            });

            var log = '[ROOM] Enter ' + ' [INFO] roomidx : ' + retVal.roomindex + ' / playerid : ' + player.id + ' / bossid : ' + retVal.bossid;
            Log(log.yellow.bold);
        }

    }
   
}

function AddPlayerToRoom(client, roomindex, pw) {
	/*
	0:����
	1101:party�� ������ �� ���� �����Դϴ�.
	1102:party�� ���� á���ϴ�.
	1104:party�� �������� �ʽ��ϴ�.
	1107:�̹� party�� �����Ͽ����ϴ�.
	1108:��Ƽ ����� Ʋ����.
	*/

    var rmInfo = Rooms.get(roomindex);

    if (rmInfo === null || rmInfo === undefined) {
        return 1104;
    }
    else {
        if (rmInfo.curplayer === ROOM_MAX) {
            return 1102;
        }
        else {
            var find = rmInfo.players.indexOf(client);

            if (find !== -1) {
                return 1107;
            }
            else if (rmInfo.pw.length > 0 && rmInfo.pw !== pw) {
                return 1108;
            }
            else {
                rmInfo.players.push(client);
                rmInfo.curplayer = rmInfo.players.length;
                Rooms.set(rmInfo.roomindex, rmInfo);
                Sockets.get(client.id).roomindex = rmInfo.roomindex;
                return 0;
            }
        }
    }
}

function LeaveRoom(client) {


    var player = Sockets.get(client.id);
    if (player.roomindex !== 0) {
        var rmInfo = Rooms.get(player.roomindex);
        if (rmInfo !== undefined) {
            var log = '[ROOM] Leave ' + ' [INFO] roomidx : ' + rmInfo.roomindex + ' / bossid : ' + rmInfo.bossid;
            Log(log.yellow.bold);

        }
    }

    var result = new Object();
    result = DelPlayerToRoom(client);
    client.emit('leaveroom', result);
    EnterSquare(client);


   
        


}
function DelPlayerToRoom(client) {
	/*
	"0:����
	1105:party���� �ƴմϴ�."
	*/

    var player = Sockets.get(client.id);
    if (player.roomindex === 0) {
        return 1105;
    }
    else {
        var rmInfo = Rooms.get(player.roomindex);
        if (rmInfo === undefined) {
            return 1105;
        }
        else {
            var find = rmInfo.players.indexOf(client);
            if (find === -1) {
                return 1105;
            }
            else {
                player.roomindex = 0;

                rmInfo.players.splice(find, 1);
                var closeParty = false;
                if (rmInfo.bossid === player.id) {
                    closeParty = true;
                }
                else {
                    if (rmInfo.players.length === 0) {
                        closeParty = true;
                    }
                    else {
                        rmInfo.curplayer = rmInfo.players.length;
                        Rooms.set(rmInfo.roomindex, rmInfo);
                    }
                }

                if (closeParty === true) {
                    //ȸ���鿡�� �˸�
                    rmInfo.players.forEach(function (item, key, mapObj) {
                        if (item.id !== client.id) {
                            Sockets.get(item.id).roomindex = 0;
                            item.emit('closedroomntf');
                            
                        }
                    });
                    //�� �ı�
                    Rooms.delete(rmInfo.roomindex);
                }
                else {
                    if (rmInfo !== null || rmInfo !== undefined) {
                        rmInfo.players.forEach(function (item, key, mapObj) {
                            if (item.id !== client.id) {
                                item.emit('leaveroomntf', player.Accountpk);
                            }
                        });

                    }
                }

                return 0;

            }
        }
    }
}


function RoomReady(client, ready) {

    var code = 0;
    var player = Sockets.get(client.id);
    var rmInfo = Rooms.get(player.roomindex);
    if (rmInfo === undefined) {
        code = 1201;
    }
    else {
        var find = rmInfo.players.indexOf(client);
        if (find === -1) {
            code = 1201;
        }
        else {

            //ȸ���鿡�� ����
            rmInfo.players.forEach(function (item, key, mapObj) {
                if (item.id !== client.id) {
                    var player = Sockets.get(item.id);
                    item.emit('roomreadyntf', player.Accountpk, ready);
                }
            });
        }
    }
    client.emit('roomready', code, ready);
}
function RoomStart(client) {

    var code = 0;
    var player = Sockets.get(client.id);
    var rmInfo = Rooms.get(player.roomindex);
    if (rmInfo === undefined) {
        code = 1201;
    }
    else {
        var find = rmInfo.players.indexOf(client);
        if (find === -1) {
            code = 1201;
        }
        else {

            //ȸ���鿡�� ����
            rmInfo.players.forEach(function (item, key, mapObj) {
                if (item.id !== client.id) {
                    var player = Sockets.get(item.id);
                    item.emit('roomstartntf');
                }
            });
        }
    }
    client.emit('roomstart', code);
}


function AddSyncObjectOfRoom(roomindex, syncid, data) {
	/*
	0:����
    1104:party�� �������� �ʽ��ϴ�.
    */

    var rmInfo = Rooms.get(roomindex);

    if (rmInfo === null || rmInfo === undefined) {
        SquareSyncObjs.set(syncid, data);
        return 1104;
    }
    else {
        rmInfo.syncobjs.set(syncid, data);
        Rooms.set(rmInfo.roomindex, rmInfo);
        return 0;
    }
}


//==========================================
//UTF8  ���ڵ�/���ڵ�
function base64encode(plaintext) {
    return Buffer.from(plaintext, "utf8").toString('base64');
}

function base64decode(base64text) {
    return Buffer.from(base64text, 'base64').toString('utf8');
}





console.log("\n\n\n---------------[START SERVER]---------------------\n".green.bold);