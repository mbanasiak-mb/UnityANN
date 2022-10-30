using System;
using System.Linq;
using System.Collections.Generic;

namespace ANN
{
    public static class LearnNetwork
    {
        private static double MixTwoGenesByCut(double geneA, double geneB)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            long geneIntA = BitConverter.DoubleToInt64Bits(geneA);
            long geneIntB = BitConverter.DoubleToInt64Bits(geneB);
            long swap;

            if (rand.Next(0, 2) == 1)
            {
                swap = geneIntA;
                geneIntA = geneIntB;
                geneIntB = swap;
            }

            long mask = 0;
            long newGene;

            int cut = rand.Next(1, 64);

            for (int i = 0; i < cut; i++)
            {
                mask |= 1L << 63 - i;
            }

            newGene = (geneIntA & mask) | (geneIntB & ~mask);
            // newGene = Mutate(newGene);

            return BitConverter.Int64BitsToDouble(newGene);
        }

        private static long Mutate(long gene)
        {
            // Don't work ?

            Random rand = new Random(Guid.NewGuid().GetHashCode());

            long mask;
            long value;

            for (int i = 0; i < 64; i++)
            {
                if (rand.Next(0, 1001) == 0)
                {
                    mask = 1L << i;
                    value = mask & gene;

                    if (value == 0)
                    {
                        gene |= mask;
                    }
                    else
                    {
                        gene &= ~mask;
                    }
                }
            }

            return gene;
        }

        public static List<List<List<double>>> MixGenes(List<List<List<double>>> weights1, List<List<List<double>>> weights2)
        {
            if (weights1.Count == weights2.Count)
            {
                for (int nLayer = 0; nLayer < weights1.Count; nLayer++)
                {
                    if (weights1[nLayer].Count == weights2[nLayer].Count)
                    {
                        for (int nNeuron = 0; nNeuron < weights1[nLayer].Count; nNeuron++)
                        {
                            if (weights1[nLayer][nNeuron].Count != weights2[nLayer][nNeuron].Count)
                            {
                                throw new Exception("Miss match of weights count in neuron.");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Miss match of neuron count in layer.");
                    }
                }
            }
            else
            {
                throw new Exception("Miss match of layers count.");
            }

            int lastLayer;
            int lastNeuron;
            double mixedWeight;
            List<List<List<double>>> mixedWeights = new List<List<List<double>>>();

            for (int nLayer = 0; nLayer < weights1.Count; nLayer++)
            {
                mixedWeights.Add(new List<List<double>>());
                lastLayer = mixedWeights.Count - 1;

                for (int nNeuron = 0; nNeuron < weights1[nLayer].Count; nNeuron++)
                {
                    mixedWeights[lastLayer].Add(new List<double>());
                    lastNeuron = mixedWeights[lastLayer].Count - 1;

                    for (int nWeight = 0; nWeight < weights1[nLayer][nNeuron].Count; nWeight++)
                    {
                        mixedWeight = MixTwoGenesByCut(weights1[nLayer][nNeuron][nWeight], weights2[nLayer][nNeuron][nWeight]);
                        mixedWeights[lastLayer][lastNeuron].Add(mixedWeight);
                    }
                }
            }

            return mixedWeights;
        }
    }

    public class NeuralNetwork
    {
        private List<Layer> Layers = new List<Layer>();

        public NeuralNetwork(int numberInputs)
        {
            if (numberInputs < 1)
            {
                string error = string.Format("Too smal amount of inputs, number of inputs = {0} must be greather than 0.", numberInputs);
                throw new Exception(error);
            }

            AddLayer(numberInputs, Functions.Linear);
        }

        public void SetInput(List<double> input)
        {
            if (input.Count != Layers[0].Neurons.Count)
            {
                string exception = string.Format("Miss match of inputs ({0} != {1}).", input.Count, Layers[0].Neurons.Count);
                throw new Exception(exception);
            }

            for (int i = 0; i < input.Count; i++)
            {
                Layers[0].Neurons[i].input = input[i];
            }
        }

        public List<double> GetOutput()
        {
            List<double> output = new List<double>();

            foreach (Neuron neuron in Layers.Last().Neurons)
            {
                output.Add(neuron.outpt);
            }

            return output;
        }

        public void SetWeights(List<List<List<double>>> weights)
        {
            if (Layers.Count < 2)
            {
                throw new Exception("Weights don't exist, network is too smal (network have less than two layers).");
            }

            if (weights.Count == Layers.Count)
            {
                for (int nLayer = 0; nLayer < weights.Count; nLayer++)
                {
                    if (weights[nLayer].Count == Layers[nLayer].Neurons.Count)
                    {
                        for (int nNeuron = 0; nNeuron < weights[nLayer].Count; nNeuron++)
                        {
                            if (weights[nLayer][nNeuron].Count != Layers[nLayer].Neurons[nNeuron].connectionInputs.Count)
                            {
                                throw new Exception("Miss match of weights count in neuron");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Miss match of neurons count in layer");
                    }
                }
            }
            else
            {
                throw new Exception("Miss match of layers count");
            }

            for (int nLayer = 0; nLayer < weights.Count; nLayer++)
            {
                for (int nNeuron = 0; nNeuron < weights[nLayer].Count; nNeuron++)
                {
                    for (int nWeight = 0; nWeight < weights[nLayer][nNeuron].Count; nWeight++)
                    {
                        Layers[nLayer].Neurons[nNeuron].connectionInputs[nWeight].weight = weights[nLayer][nNeuron][nWeight];
                    }
                }
            }
        }

        public List<List<List<double>>> GetWeights()
        {
            if (Layers.Count < 2)
            {
                string error = "Weights don't exist, network is too smal.";
                throw new Exception(error);
            }

            int lastLayer;
            int lastNeuron;
            List<List<List<double>>> weights = new List<List<List<double>>>();

            foreach (Layer layer in Layers)
            {
                weights.Add(new List<List<double>>());

                foreach (Neuron neuron in layer.Neurons)
                {
                    lastLayer = weights.Count - 1;
                    weights[lastLayer].Add(new List<double>());

                    foreach (Connection connection in neuron.connectionInputs)
                    {
                        lastNeuron = weights[lastLayer].Count - 1;
                        weights[lastLayer][lastNeuron].Add(connection.weight);
                    }
                }
            }

            return weights;
        }

        public void FeedForward()
        {
            foreach (Layer layer in Layers)
            {
                foreach (Neuron neuron in layer.Neurons)
                {
                    neuron.CollectInpulses();
                    neuron.AcivateFunction();
                }
            }
        }

        public void AddLayer(int amountNeurons, Func<double, double> func)
        {
            Layers.Add(new Layer(amountNeurons, func));

            if (Layers.Count > 1)
            {
                int lastLayer = Layers.Count - 1;
                ConnectLayers(Layers[lastLayer - 1], Layers[lastLayer]);
            }
        }

        public void InitializeWeights()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            foreach (Layer layer in Layers)
            {
                foreach (Neuron neuron in layer.Neurons)
                {
                    foreach (Connection connection in neuron.connectionInputs)
                    {
                        connection.weight = rand.NextDouble() * 2 - 1;
                    }
                }
            }
        }

        private void ConnectLayers(Layer layerA, Layer layerB)
        {
            foreach (Neuron neuronA in layerA.Neurons)
            {
                foreach (Neuron neuronB in layerB.Neurons)
                {
                    Connection connection = new Connection();

                    neuronA.connectionOutputs.Add(connection);
                    neuronB.connectionInputs.Add(connection);
                    connection.neuronA = neuronA;
                    connection.neuronB = neuronB;
                }
            }
        }

        private class Layer
        {
            public List<Neuron> Neurons = new List<Neuron>();

            public Layer(int amountNeurons, Func<double, double> func)
            {
                if (amountNeurons < 1)
                {
                    string error = string.Format("Too smal amount of neurons, number of neurons = {0} must be greather than 0.", amountNeurons);
                    throw new Exception(error);
                }

                for (int i = 0; i < amountNeurons; i++)
                {
                    Neurons.Add(new Neuron(func));
                }
            }
        }

        private class Neuron
        {
            public double input;
            public double outpt;
            public List<Connection> connectionInputs;
            public List<Connection> connectionOutputs;
            private ActivationFunctionDelegate acivationFunction;
            private delegate double ActivationFunctionDelegate(double x);

            public Neuron(Func<double, double> func)
            {
                input = 0;
                outpt = 0;
                connectionInputs = new List<Connection>();
                connectionOutputs = new List<Connection>();
                SetActivationFunction(func);
            }

            public void CollectInpulses()
            {
                if (connectionInputs.Count > 0)
                {
                    input = 0;

                    foreach (Connection connection in connectionInputs)
                    {
                        input += connection.neuronA.outpt * connection.weight;
                    }
                }
            }

            public void AcivateFunction()
            {
                outpt = acivationFunction.Invoke(input);
            }

            public void SetActivationFunction(Func<double, double> func)
            {
                acivationFunction = new ActivationFunctionDelegate(func);
            }
        }

        private class Connection
        {
            public double weight;
            public Neuron neuronA;
            public Neuron neuronB;

            public Connection()
            {
                weight = 0;
            }
        }
    }
}
