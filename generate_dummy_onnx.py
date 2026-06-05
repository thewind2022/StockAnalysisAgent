import onnx
from onnx import helper, TensorProto
import numpy as np

# 定义输入：input_ids (int64, 形状 [1, sequence_length])
input_tensor = helper.make_tensor_value_info('input_ids', TensorProto.INT64, [1, 'seq_len'])

# 定义输出：output (float32, 形状 [1, 1])，模拟风险分数
output_tensor = helper.make_tensor_value_info('output', TensorProto.FLOAT, [1, 1])

# 创建一个常数输出节点（恒输出 0.5）
# 使用 ConstantOfShape + 数值
constant_value = np.array([0.5], dtype=np.float32)
constant_tensor = helper.make_tensor('constant_value', TensorProto.FLOAT, [1], constant_value)
constant_node = helper.make_node('Constant', [], ['output'], value=constant_tensor, name='ConstantNode')

# 也可使用 Identity（输入直接作为输出，但需确保类型一致，这里用 Constant 更简单）
# 为使模型更真实，添加一个 Identity 节点连接 input_ids -> output，但会改变输出形状，故选择常数输出

graph = helper.make_graph(
    [constant_node],
    'dummy_sentiment_model',
    [input_tensor],      # 输入列表
    [output_tensor]      # 输出列表
)

model = helper.make_model(graph, producer_name='dummy_onnx_generator', opset_imports=[helper.make_opsetid('', 14)])

onnx.save(model, 'sentiment_model.onnx')
print("✅ 已生成 sentiment_model.onnx，输出恒为 0.5")