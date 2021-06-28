# huawei-nlp
#在完成了阿里百度腾讯有道搜狗讯飞字节火山等等的对接挑战之后，今天来处理华为的机器翻译对接

1. 还是先申请华为的接口[https://support.huaweicloud.com/nlp/index.html](https://support.huaweicloud.com/nlp/index.html)
注册申请后，去控制台>我的凭证 建立项目并下载凭证（AK/SK）,同时记下建立的项目ID: ProjectID以及项目所在区域，我这里是	cn-north-4
```c#
const string Region = "cn-north-4";
const string ProjectID= "0cbfb339b0********e7c01fd250fb19"
const string AK = "WAU6RWK********UDB3I";
const string SK = "3cUvdgM4K9R1**************hHHkwSxQ1mgwC"
```
（华为的API有两种签名方法，一种是基于用户名密码的签名好像是24小时时效，不方便；另一种是AK/SK）
2. 机器翻译服务接口说明文档在 [https://support.huaweicloud.com/api-nlp/nlp_03_0024.html](https://support.huaweicloud.com/api-nlp/nlp_03_0024.html) 并且提供了JAVA和Python的SDK
做了这么多接入之后总结下来基本要做的就是就是按照输入输出格式，和签名方法进行适配
3. 语种说明
语言|说明
---|---
zh|中文
en|英文
ja|日文
ru|俄文
ko|韩语
fr|法语
es|西班牙语
de|德语
ar|阿拉伯语
auto|自动检测输入语种并翻译成目标语种，您需要指定目标语种。
4.  Request结构
```json
{
    "text": "欢迎使用机器翻译服务",
    "from": "zh",
    "to": "en",
    "scene":"common"
}  
```
5. Response结构
 ```json
{
  "src_text": "欢迎使用机器翻译服务",
    "translated_text": "Welcome to use machine translation services",
    "from": "zh",
    "to": "en"
} 
```
6. 签名方法 
华为AK/SK签名方法：[https://support.huaweicloud.com/devg-apisign/api-sign-algorithm-002.html](https://support.huaweicloud.com/devg-apisign/api-sign-algorithm-002.html)
我本来计划是和字节火山一样自己重写一个签名方法，因为华为用的也是CanonicalRequest加SignedHeaders等等，但是发现虽然NLP没有.net样例 但是签名有单独的SDK样例
[https://support.huaweicloud.com/devg-apisign/api-sign-sdk-csharp.html](https://support.huaweicloud.com/devg-apisign/api-sign-sdk-csharp.html) 这我就方便了（我想早了）
下载链接 [https://obs.cn-north-1.myhuaweicloud.com/apig-sdk/APIGW-csharp-sdk.zip](https://obs.cn-north-1.myhuaweicloud.com/apig-sdk/APIGW-csharp-sdk.zip)
主要是两个文件 HttpEncoder.cs 和 Signer.cs
通过
```c#
signer = new Signer
{
   //Set the AK/SK to sign and authenticate the request.
   Key = AK,
   Secret = SK
};
//构建URL
URL = "https://nlp-ext."+Region+".myhuaweicloud.com/v1/" + ProjectID + "/machine-translation/text-translation";
HttpRequest r = new HttpRequest("POST", new Uri(URL));
r.headers.Add("Content-Type", "application/json");
r.headers.Add("X-Project-Id", ProjectID);
r.body = requestBody;
HttpWebRequest request = signer.Sign(r);
```
之后就可以正常发送请求了。
令人困惑的是
算法说明页面
[https://support.huaweicloud.com/devg-apisign/api-sign-algorithm-005.html](https://support.huaweicloud.com/devg-apisign/api-sign-algorithm-005.html)的样例中给出示例
得到的签名消息头为：
```
SDK-HMAC-SHA256 Access=QTWAOYTTINDUT2QVKYUC, SignedHeaders=content-type;host;x-sdk-date, Signature=7be6668032f70418fcc22abc52071e57aff61b84a1d2381bb430d6870f4f6ebe
```
然而Signer.cs中定义是
```c#
 readonly HashSet<string> unsignedHeaders = new HashSet<string> { "content-type" };
```
经测试最终还是Signer.cs中的正确
另外给的样例中没有提到要加这个header
```
r.headers.Add("X-Project-Id", ProjectID);
```
我也是偶然试出来需要这个才行

