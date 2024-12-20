ARG NAME=coreutils
ARG VERSION=9.5

RUN wget https://ftp.gnu.org/gnu/coreutils/coreutils-$VERSION.tar.xz && \
    tar xf coreutils-$VERSION.tar.xz

WORKDIR /coreutils-$VERSION

RUN LDFLAGS=-static FORCE_UNSAFE_CONFIGURE=1 ./configure --prefix=/tmp/coreutils && \
    make -j`nporc` && \
    make install && \
    strip -s /tmp/coreutils/bin/*

WORKDIR /app

RUN cp /tmp/coreutils/bin/* . && \
    cp /coreutils-$VERSION/COPYING .