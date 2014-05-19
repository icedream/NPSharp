# Master API source code

This folder is supposed to contain the source code that will provide both, a master server and client.

alterIWnet and fourdeltaone made use of the open-sourced DP master server code to serve registration of dedicated servers
and the dedicated server list for the game client.

This server is supposed to be replaced by a full implementation of the protocol, both as server and client, in C# and is
to be integrated into libnpsharp.

## Protocol

The master server protocol is based on Quake 3's:

Request message:
	AA AA AA AA|BB .. .. BB|		A=Header, B=Body ending with line break or \x00

Response message:
	AA .. .. AA|0a|BB .. ..			A=Message name, B=Body
	BB

Important: The response is supposed to be one or multiple specific-length plus a less-than-specific-length
UDP packet(s).

TODO: Look up the specific length in the DP master source code. It's somewhere around 1400/1500 bytes.

The master server itself has to only serve for the "getservers" request and has to respond with a getServersResponse message.
This getServersResponse message's body is serialized as following:

	XX AA AA AA AA BB BB[CC			X=ascii("\\"), A=IP (uint, network byte order), B=Port 1 (ushort, network byte order), C=Optional port 2 (ushort, network byte order)
	CC[..]  ]XX .. .. .. ..
	XX YY YY YY	00 00 00[00			Y=ascii("EOT")
	00]

In short, serialized ip-port pairs/triples separated by ASCII backslashes and ended by an entry which decodes to "EOT".

## Protocol limits

(see https://github.com/kphillisjr/dpmaster/blob/master/src/messages.c for reference)

```c
// Timeout after a valid infoResponse (in secondes)
#define TIMEOUT_INFORESPONSE (15 * 60)

// Period of validity for a challenge string (in secondes)
#define TIMEOUT_CHALLENGE 2

// Maximum size of a reponse packet
#define MAX_PACKET_SIZE_OUT 1400
```

## Internal management

Both the server and the client should be able to handle a variable amount of ports. For this, we plan following things:

1) Assume "\" at the beginning of an entry. If not true, stream out of sync -> throw exception/disconnect.
2) Assume an IP address exists (4 bytes minimum).
	a) If IP address is ASCII "EOT" with \x00 bytes at the end, stop reading the message.
3) Do not assume a fixed amount of ports. Instead
	a) ...if rest > 0 bytes, read 2 bytes and append to a ushort list for ports.
	b) ...if rest == 0 bytes, prepare for reading next entry.

This should be compatible with both, messages with 1 IP address + 1 port (game) and 1 IP address + 2 ports (game/query).

## Messages

(see https://github.com/kphillisjr/dpmaster/blob/master/src/messages.c for reference)

// Types of messages (with samples):

// Q3: "heartbeat QuakeArena-1\x0A"
// DP: "heartbeat DarkPlaces\x0A"
#define S2M_HEARTBEAT "heartbeat "

// Q3 & DP & QFusion: "getinfo A_Challenge"
#define M2S_GETINFO "getinfo"

// Q3 & DP & QFusion: "infoResponse\x0A\\pure\\1\\..."
#define S2M_INFORESPONSE "infoResponse\x0A"

// Q3: "getservers 67 ffa empty full"
// DP: "getservers DarkPlaces-Quake 3 empty full"
// DP: "getservers Transfusion 3 empty full"
// QFusion: "getservers qfusion 39 empty full"
#define C2M_GETSERVERS "getservers "

// DP: "getserversExt DarkPlaces-Quake 3 empty full ipv4 ipv6"
// IOQuake3: "getserversExt 68 empty ipv6"
#define C2M_GETSERVERSEXT "getserversExt "

// Q3 & DP & QFusion:
// "getserversResponse\\...(6 bytes)...\\...(6 bytes)...\\EOT\0\0\0"
#define M2C_GETSERVERSREPONSE "getserversResponse"

// DP & IOQuake3:
// "getserversExtResponse\\...(6 bytes)...//...(18 bytes)...\\EOT\0\0\0"
#define M2C_GETSERVERSEXTREPONSE "getserversExtResponse"
