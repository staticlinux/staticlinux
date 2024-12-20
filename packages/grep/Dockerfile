ARG NAME=grep
ARG VERSION=3.11

RUN wget https://ftp.gnu.org/gnu/grep/grep-$VERSION.tar.xz && \
    tar xf grep-$VERSION.tar.xz 

WORKDIR /grep-$VERSION

RUN LDFLAGS=-static ./configure && \
    make -j`nporc` && \
    strip -s ./src/grep

WORKDIR /app

RUN cp /grep-$VERSION/src/grep . && \
    cp /grep-$VERSION/COPYING .