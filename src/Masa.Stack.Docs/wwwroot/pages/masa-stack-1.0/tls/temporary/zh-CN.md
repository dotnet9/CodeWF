## 创建临时证书 

生成临时的`tls证书`提供给`ingress`使用

* Country Name 国家名称： ZH
* State or Province Name 省份： ZheJiang
* Locality Name  城市： WenZhou
* Organization Name 组织名称/公司名称： Masastack
* Organizational Unit Name  组织单位名称/公司部门: Masastack
* Common Name       域名： *.masastack.com  (这里是域名 *代表的是泛域名)
* Email Address    邮箱地址： 123@masastack.com

```shell
[root@a.test]# openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout tls.key -out tls.crt
Generating a 2048 bit RSA private key
...........................................+++
........................................................+++
writing new private key to 'tls.key'
-----
You are about to be asked to enter information that will be incorporated
into your certificate request.
What you are about to enter is what is called a Distinguished Name or a DN.
There are quite a few fields but you can leave some blank
For some fields there will be a default value,
If you enter '.', the field will be left blank.
-----
Country Name (2 letter code) [XX]:ZH
State or Province Name (full name) []:ZheJiang
Locality Name (eg, city) [Default City]:WenZhou
Organization Name (eg, company) [Default Company Ltd]:Masastack
Organizational Unit Name (eg, section) []:Masastack
Common Name (eg, your name or your server's hostname) []:*.masastack.com
Email Address []:123@masastack.com
```

```shell
[root@a.test]# ls
tls.crt  tls.key
```

```shell
[root@a.test]# cat tls.crt 
-----BEGIN CERTIFICATE-----
MIIEATCCAumgAwIBAgIJAJOEQs4wPXutMA0GCSqGSIb3DQEBCwUAMIGWMQswCQYD
VQQGEwJaSDERMA8GA1UECAwIWmhlSmlhbmcxEDAOBgNVBAcMB1dlblpob3UxEjAQ
BgNVBAoMCU1hc2FzdGFjazESMBAGA1UECwwJTWFzYXN0YWNrMRgwFgYDVQQDDA8q
Lm1hc2FzdGFjay5jb20xIDAeBgkqhkiG9w0BCQEWETEyM0BtYXNhc3RhY2suY29t
MB4XDTIzMDEwOTE1MTcxNFoXDTI0MDEwOTE1MTcxNFowgZYxCzAJBgNVBAYTAlpI
MREwDwYDVQQIDAhaaGVKaWFuZzEQMA4GA1UEBwwHV2VuWmhvdTESMBAGA1UECgwJ
TWFzYXN0YWNrMRIwEAYDVQQLDAlNYXNhc3RhY2sxGDAWBgNVBAMMDyoubWFzYXN0
YWNrLmNvbTEgMB4GCSqGSIb3DQEJARYRMTIzQG1hc2FzdGFjay5jb20wggEiMA0G
CSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC/ziMVhIKDcq4vKMniTeN2k6fUcNn1
mnyBmgVql8VK+GHplK+AuMmXEUG0qZyf+69ckD/PtKmDUMQvk3zoU3/MkBh4DdUs
Zfs61/iUy4ZRkvraMZQrzOmZ/B6nG9pqvzeopicGHsDz8GVpaC3qLysJZsV3PaNh
3tLoPlETODRkAAvWYzlMEdorhzg375Y30uXap2eGEYfYSDyyvD0LZveyfLVBm6iJ
9uQ86MLf4U3nKnYTKh6XsurZxke6K4gMm++SilmeUOPWwUcqGv3Y8mP05TSaOv30
fBYVsvlHq6ah3H5T3WoiZk1H5IJxUfJFEe6XfZ3SpcQ1wCljHKKmOodNAgMBAAGj
UDBOMB0GA1UdDgQWBBTgwBY9JasUUUT5omN7HfMxZEWO0TAfBgNVHSMEGDAWgBTg
wBY9JasUUUT5omN7HfMxZEWO0TAMBgNVHRMEBTADAQH/MA0GCSqGSIb3DQEBCwUA
A4IBAQCrfhnfg7HhLQ7sNxlaSlKqDi6il7AXrDq7z/xdv17NVXEKCCxq4wSJq9zG
/x3pe5sDd4LiT0oYm9zl17LTiIK90nREqx0YSBgCh10y1j1chihHNso4FLqs5Esg
FpLXk1cnr440mXluQLxUJt+pzdd1LAE7UDRmyZZAJdJWrdmFkNNhGQWeTXROgznb
PFCP3UqsG+jhkRrFqOjMtJQXmj8AZa1J14yv5aTUVzkErVS4VqngNK+ETTGNiaXC
0faqJ49yLThphbVhvx9aGqlru34EmXfsp8h+VQRh1pVi/MNZcIFtcIh6GvMWNGlX
gAcaZhAEE95MM16OmVXyKDgtqTtu
-----END CERTIFICATE-----
[root@a.test]# cat tls.key 
-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQC/ziMVhIKDcq4v
KMniTeN2k6fUcNn1mnyBmgVql8VK+GHplK+AuMmXEUG0qZyf+69ckD/PtKmDUMQv
k3zoU3/MkBh4DdUsZfs61/iUy4ZRkvraMZQrzOmZ/B6nG9pqvzeopicGHsDz8GVp
aC3qLysJZsV3PaNh3tLoPlETODRkAAvWYzlMEdorhzg375Y30uXap2eGEYfYSDyy
vD0LZveyfLVBm6iJ9uQ86MLf4U3nKnYTKh6XsurZxke6K4gMm++SilmeUOPWwUcq
Gv3Y8mP05TSaOv30fBYVsvlHq6ah3H5T3WoiZk1H5IJxUfJFEe6XfZ3SpcQ1wClj
HKKmOodNAgMBAAECggEAURwY1gadMn0Sj7rN9Lc/U2uJc1rtsODNefjqBXN86QE0
VpSbyvFZvlp70KxRIY5LT/doJKufa3qCHCRgk9aLmrPsxQgEd6wAm5es9S9D88cV
8aM5p3QV7Roi1EQBD1chcF4i7oGe0wl7uSFnGTstFeKx6oTUTJTv12pS2q/P5+Ep
AIO+y8uyVWMe3xYAKi00y9ewPtxX42T9lRDqGrzfo8OnxPImH+Z4JLkKixZTUsqk
ipldq5AmUGLKTJ5yfd+0XKVGqZxU17QHrMwfX/tlEBhURnFzihSi2NfTX/p0KAnm
5bQxHDzkY5grzyj0pXjaB6YgPHdSCLdq/lQNJlO5QQKBgQDxwcRXf9jp3AWlNY8L
ho0cjB12pBsLScWSfkoeYbFy8x/+VqFbdeI5O0GoSxRpJq4CppwEgJzdqNUuHubZ
39g/XQdwKLtMjpa49poTqMYT4UnMoPb502U+R7j/26hyltzdU5uufM3lfX7fhC5W
HtkQAptGSKPj7s4JWUgHxh93KQKBgQDLGvzGfy32thAL7CTzQYuOJFt1YEkUKQD7
4TbE5iE9SFaHp57qrujKdRakAuGi9EUufAmZ1Qqw3pwOUviS8MttSpzNDlJXAlMr
ASAuYFQrwraUHtOi69Nr2EwlgPIThgAsq9ZL9wyMXY8d9CJDW3hVbEHpQoPzKFtz
Ust3CBOHhQKBgQCCxDWowp2Y+YsQLuU97by8WUnCl8eNFo1IzQjYYC10qO+ASmmj
KCOCo3vDRUE4E1UCWA6CHPM8rosJFGv4I607sN1KHK4bHfGHANScl6j0reKWTebp
gR/9TRxTQQRfXxz+lq/Z9OYGIRiUXFIYAT2V/GLy5G3J560Iv4NHuTHh6QKBgQC8
TEXjZU9gtgQEeab8G11dp5lfJag615UA8BhNzaktXp5SX/W2T/ikko8t+TnlUJ++
6+Iey2OA/LEjmoq3+CQxLAZZGZj+77nZWc7TEB2ZAIkyo63EEuzMxBg8gOJtdUM+
JwWjIeRxUd/4fjkxx2C1mYs1zaP1UAoQzcaykTtB3QKBgEkuz/m+4YoBZDrijtJg
QPFxv48JdNNaMvN4CzFk94z9CuPZcdYoRmL1+xZNUM0eUbRG88dZgswhqXUHD067
OHoUyq+WExA99lTAGaRvGgnjJTxt8E2tZpHhV55IoPhJJFNtSN/aB1PcGuZ7+TNc
vbRG7sTyTM2AkQiL0PLWQBBv
-----END PRIVATE KEY-----
```