# EscudeEditor
A Tool to edit the Escude Bin scripts and packgets


## Note for devlopers:
-The Escude engine use the file index to read a bin packget,
he can contains the file name but the file order is what the
game uses, so, to create a new readable packget you need keep
the original file order.

-This tool don't include a recompressor algorithm, just a fake
compressor, since my game worked without the compression the
fake compressor is disabled by default. (Create bigger files)

-The Decompressor algorithm have a big part copied of the GARBro
tool, Special thanks to the morkt!