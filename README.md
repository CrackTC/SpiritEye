# SpiritEye
## Usage

```shell
# scan
spiriteye scan <target> -p <ports> -o <path>
spiriteye scan 192.168.1.1/24 -p - -o result.json # '-' for all ports

# merge
spiriteye merge <results> -o <path>
spiriteye merge result1.json result2.json -o result.json
```
