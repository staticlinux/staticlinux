FROM alpine:3

RUN apk add binutils gcc musl-dev make

RUN wget https://ftp.gnu.org/gnu/grep/grep-3.11.tar.xz && \
    tar xf grep-3.11.tar.xz 

WORKDIR /grep-3.11

RUN LDFLAGS=-static ./configure && \
    make -j`nporc` && \
    strip -s ./src/grep

FROM scratch
WORKDIR /bin
COPY --from=0 /grep-3.11/src/grep /bin/