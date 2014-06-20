APP_SRC = usr/share/autologin-on-boot/
APP_DEST = ${DESTDIR}/usr/share/autologin-on-boot/

BIN_SRC = usr/bin/
BIN_DEST = ${DESTDIR}/usr/bin/

.PHONY: install

install:
	install -d $(APP_DEST)
	install -m755 $(APP_SRC)booting $(APP_DEST)
	install -m755 $(APP_SRC)profile $(APP_DEST)
	install -m755 $(APP_SRC)getty $(APP_DEST)

	install -d $(BIN_DEST)
	install -m755 $(BIN_SRC)gcaliper $(BIN_DEST)
