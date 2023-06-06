using System;

namespace Drm.Format.EReader;

[Flags]
public enum PdbAttributes : ushort
{
	Unknown0				= 0x0001,
	ReadOnly				= 0x0002,
	DirtyAppInfoArea		= 0x0004,
	BackupThisDb			= 0x0008,
	OkToReplaceWithNewerVer = 0x0010,
	ForceRestartAfterInstall= 0x0020,
	PreventFileSharing		= 0x0040,
	Unknown7				= 0x0080,
	Unknown8				= 0x0100,
	Unknown9				= 0x0200,
	Unknown10				= 0x0400,
	Unknown11				= 0x0800,
	Unknown12				= 0x1000,
	Unknown13				= 0x2000,
	Unknown14				= 0x4000,
	Unknown15				= 0x8000,
}

[Flags]
public enum PdbRecordAttributes : byte
{
	Secret		= 0x1,
	InUse		= 0x2,
	Dirty		= 0x4,
	DeleteOnSync= 0x8,
}