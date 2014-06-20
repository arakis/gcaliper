APP_SRC = src/
APP_DEST = ${DESTDIR}/usr/share/gcaliper/

BIN_SRC = contrib/bin/
BIN_DEST = ${DESTDIR}/usr/bin/

#APP_SRC = src/
#APP_DEST = /tmp/make/usr/share/gcaliper/

#BIN_SRC = usr/bin/
#BIN_DEST = /tmp/make/usr/bin/

.PHONY: install

install:
	mdtool build solution.sln

	install -d $(APP_DEST)
	install -d $(APP_DEST)bin
	
	install -m755 $(APP_SRC)bin/gcaliper.exe $(APP_DEST)bin
	install -m644 $(APP_SRC)bin/gcaliper.exe.mdb $(APP_DEST)bin
	install -m644 $(APP_SRC)appicon.ico $(APP_DEST)
	
	install -d $(APP_DEST)themes/caliper
	install -D -m644 $(APP_SRC)themes/caliper/*.png $(APP_DEST)themes/caliper
	install -D -m644 $(APP_SRC)themes/caliper/*.conf $(APP_DEST)themes/caliper

	install -d $(BIN_DEST)
	install -m755 $(BIN_SRC)gcaliper $(BIN_DEST)
