FROM alpine:3

RUN apk update && \
    apk add bash binutils gcc linux-headers musl-dev make

COPY <<-"EOT" /bin/gcc-wrapper
#!/bin/bash
args=("$@")
for ((i=0; i<"${#args[@]}"; ++i)); do
    case ${args[i]} in
        -all-static) unset args[i]; break;;
    esac
done
gcc "${args[@]}"
EOT

RUN chmod +x /bin/gcc-wrapper

RUN wget https://www.kernel.org/pub/linux/utils/util-linux/v2.40/util-linux-2.40.tar.gz && \
    tar xf util-linux-2.40.tar.gz

WORKDIR /util-linux-2.40

RUN CC=/bin/gcc-wrapper LDFLAGS="-all-static" ./configure --enable-shared=no --disable-liblastlog2 --prefix=/tmp/util-linux && \
    make -j`nproc` && \
    make install && \
    strip -s /tmp/util-linux/bin/*

FROM scratch
WORKDIR /bin
COPY --from=0 /tmp/util-linux/bin/* /bin/