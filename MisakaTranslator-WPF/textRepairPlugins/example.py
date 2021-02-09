# coding=utf-8
# 不加上面那行就不能写中文注释
import re
def process(source):
    #去除HTML标签
    source = re.sub("<[^>]+>", "", source)
    source = re.sub("&[^;]+;", "", source)
    #句子去重（根据空格分割句子，取最后一段）
    parts = [p for p in source.split(" ") if len(p) > 0]
    return parts[-1] if len(parts) != 0 else ""